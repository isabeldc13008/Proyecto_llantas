import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

type Item={id:string;brand:string;reference:string;dot:string;dimension:string;retreadBand:string;outer:number;center:number;inner:number;cause:string;decision:string;origin:string;destination:string;status:string;date:string;selected?:boolean};

@Component({selector:'app-disposition',imports:[FormsModule],templateUrl:'./disposition.html',styleUrl:'./disposition.scss'})
export class Disposition{
 items=signal<Item[]>(this.load()); filter='Todos'; message='';
 visible(){return this.items().filter(i=>i.decision==='Disposición final'&&(this.filter==='Todos'||i.origin===this.filter));}
 received(item:Item){this.update(item,{status:'Recibida en R1',destination:'R1'});}
 toggle(item:Item,value:boolean){this.update(item,{selected:value});}
 deliver(){const selected=this.items().filter(i=>i.selected&&['Recibida en R1','Lista para acta'].includes(i.status));if(!selected.length){this.message='Selecciona al menos una llanta disponible en R1.';return}this.downloadAct(selected);this.items.update(all=>all.map(i=>selected.includes(i)?{...i,status:'Entregada a empresa de disposición',selected:false}:i));this.save();this.message=`Acta descargada con ${selected.length} llanta(s). Revisa la carpeta Descargas de tu equipo.`;}
 private downloadAct(items:Item[]){
  const doc=new jsPDF({orientation:'landscape',unit:'mm',format:'a4'});
  const date=new Date().toLocaleDateString('es-CO');
  doc.setFillColor(7,59,109);doc.rect(0,0,297,24,'F');
  doc.setTextColor(255,255,255);doc.setFont('helvetica','bold');doc.setFontSize(15);doc.text('GLLD · EDINSA',12,10);doc.setFontSize(11);doc.text('ACTA DE DISPOSICION FINAL PARA LLANTAS',12,18);
  doc.setTextColor(30,48,61);doc.setFontSize(9);doc.text(`FECHA: ${date}`,12,31);doc.text('SEDE QUE ENTREGA: R1',70,31);doc.text('Recoleccion autorizada unicamente en planta R1',145,31);
  autoTable(doc,{startY:36,theme:'grid',styles:{fontSize:6.5,cellPadding:1.5,valign:'middle'},headStyles:{fillColor:[139,197,63],textColor:[7,52,89],fontStyle:'bold'},head:[['Item','ID LLANTA\n(# marcacion)','Marca','Referencia\nDiseno original','DOT','Dimension','Banda reencauche\nDiseno','Prof.\next.','Prof.\ncent.','Prof.\nint.','Motivo / dano / causa','Planta\norigen']],body:items.map((i,index)=>[index+1,i.id,i.brand,i.reference,i.dot,i.dimension,i.retreadBand||'N/A',i.outer,i.center,i.inner,i.cause,i.origin]),margin:{left:12,right:12}});
  const end=(doc as any).lastAutoTable.finalY+12;
  doc.setFontSize(8);doc.setFont('helvetica','normal');doc.text('Nombre de quien entrega: ____________________________________',16,end);doc.text('Nombre de quien recibe: ____________________________________',155,end);
  doc.text('Firma: _____________________________________________________',16,end+10);doc.text('Firma: _____________________________________________________',155,end+10);
  doc.text('Placa del vehiculo recolector: _______________________________',155,end+20);
  doc.setFontSize(6.5);doc.setTextColor(95,112,123);doc.text('Documento generado por GLLD. Conserva la trazabilidad de la planta que origino cada llanta (R1, R2, R3 o R4).',12,202);
  doc.save(`Acta_disposicion_R1_${new Date().toISOString().slice(0,10)}.pdf`);
 }
 private update(item:Item,changes:Partial<Item>){this.items.update(all=>all.map(i=>i===item?{...i,...changes}:i));this.save();}
 private save(){localStorage.setItem('glld_disposal_queue',JSON.stringify(this.items()));}
 private load():Item[]{try{const saved=JSON.parse(localStorage.getItem('glld_disposal_queue')??'[]');if(saved.length)return saved}catch{}return[
  {id:'LL-000118',brand:'Michelin',reference:'X Multi D',dot:'1823',dimension:'295/80 R22.5',retreadBand:'XDA2',outer:2,center:2.5,inner:2,cause:'Daño estructural',decision:'Disposición final',origin:'R2',destination:'R1',status:'Pendiente envío a R1',date:'10/08/2026'},
  {id:'LL-000076',brand:'Goodyear',reference:'KMAX D',dot:'0922',dimension:'295/80 R22.5',retreadBand:'KMAX D',outer:1.5,center:2,inner:1.5,cause:'Fin de vida útil',decision:'Disposición final',origin:'R3',destination:'R1',status:'Recibida en R1',date:'09/08/2026'},
  {id:'LL-000055',brand:'Bridgestone',reference:'R268',dot:'3121',dimension:'11 R22.5',retreadBand:'R297',outer:0,center:1,inner:0,cause:'Accidente',decision:'Disposición final',origin:'R1',destination:'',status:'Lista para acta',date:'08/08/2026'}];}
}
