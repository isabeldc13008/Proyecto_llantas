import {ActaSnapshot} from './disposition-models';
export async function downloadActa(snapshot:ActaSnapshot){
 const[{jsPDF},{default:autoTable}]=await Promise.all([import('jspdf'),import('jspdf-autotable')]);const doc=new jsPDF();
 doc.setFontSize(16);doc.text('Acta de entrega para disposición final',14,20);doc.setFontSize(9);
 const date=(v:string)=>new Date(v).toLocaleString('es-CO');
 const lines=[snapshot.codigo,`Emisión: ${date(snapshot.fechaEmision)}`,`Centro de origen: ${snapshot.centroOrigen}`,`Centro R1: ${snapshot.centroR1}`,`Empresa receptora: ${snapshot.proveedor}`,`Salida: ${date(snapshot.fechaSalida)}`,`Transportador: ${snapshot.transportador} · Placa: ${snapshot.placa}`,`Remisión: ${snapshot.remision||'Sin remisión'}`];
 let y=29;for(const line of lines){const wrapped=doc.splitTextToSize(line,180);doc.text(wrapped,14,y);y+=wrapped.length*4.5+2;}
 autoTable(doc,{startY:y+3,head:[['Llanta','Serial','Marca / dimensión','Lote de entrada']],body:snapshot.llantas.map(t=>[t.codigo,t.serial,`${t.marca} / ${t.dimension}`,t.loteEntrada]),styles:{fontSize:8,overflow:'linebreak'},headStyles:{fillColor:[12,93,12]},margin:{bottom:25}});
 y=(doc as any).lastAutoTable.finalY+18;if(y>250){doc.addPage();y=25}doc.text(`Total: ${snapshot.llantas.length} llantas`,14,y);doc.text('Entrega: ______________________     Recibe: ______________________',14,y+15);doc.text('Firma y fecha: __________________     Firma y fecha: __________________',14,y+27);
 for(let n=1;n<=doc.getNumberOfPages();n++){doc.setPage(n);doc.setFontSize(8);doc.text(`${snapshot.codigo} · ${n}/${doc.getNumberOfPages()}`,14,287)}
 doc.save(snapshot.codigo+'.pdf');
}
