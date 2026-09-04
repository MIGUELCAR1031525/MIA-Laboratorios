from pathlib import Path

source = Path(__file__).with_name('solucion_lab9.md')
output = Path(__file__).with_name('solucion_lab9.pdf')
text = source.read_text(encoding='utf-8')
lines = []
for raw in text.splitlines():
    line = raw.replace('#', '').replace('`', '').replace('**', '')
    if line.startswith('|'):
        line = line.replace('|', '  ')
    lines.append(line)
page_lines = 48
pages = [lines[i:i + page_lines] for i in range(0, len(lines), page_lines)] or [[]]
objects = []
objects.append('<< /Type /Catalog /Pages 2 0 R >>')
page_refs = ' '.join(f'{4 + i * 2} 0 R' for i in range(len(pages)))
objects.append(f'<< /Type /Pages /Kids [{page_refs}] /Count {len(pages)} >>')
objects.append('<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>')
for index, page in enumerate(pages):
    content_ref = 5 + index * 2
    objects.append(f'<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R >> >> /Contents {content_ref} 0 R >>')
    commands = ['BT', '/F1 9 Tf', '45 755 Td', '12 TL']
    for line in page:
        safe = line.encode('cp1252', 'replace').decode('cp1252').replace('\\', '\\\\').replace('(', '\\(').replace(')', '\\)')
        commands.append(f'({safe[:105]}) Tj')
        commands.append('0 -12 Td')
    commands.append('ET')
    stream = '\n'.join(commands).encode('cp1252')
    objects.append(f'<< /Length {len(stream)} >>\nstream\n{stream.decode("cp1252")}\nendstream')
pdf = bytearray(b'%PDF-1.4\n')
offsets = [0]
for number, obj in enumerate(objects, 1):
    offsets.append(len(pdf))
    pdf.extend(f'{number} 0 obj\n{obj}\nendobj\n'.encode('cp1252'))
xref = len(pdf)
pdf.extend(f'xref\n0 {len(objects) + 1}\n0000000000 65535 f \n'.encode('ascii'))
for offset in offsets[1:]:
    pdf.extend(f'{offset:010d} 00000 n \n'.encode('ascii'))
pdf.extend(f'trailer\n<< /Size {len(objects) + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n'.encode('ascii'))
output.write_bytes(pdf)
