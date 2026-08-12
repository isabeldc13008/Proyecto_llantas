import { ComponentFixture, TestBed } from '@angular/core/testing';
import { VehicleAxleDiagram } from './vehicle-axle-diagram';

describe('VehicleAxleDiagram',()=>{
 let fixture:ComponentFixture<VehicleAxleDiagram>;
 beforeEach(async()=>{await TestBed.configureTestingModule({imports:[VehicleAxleDiagram]}).compileComponents();fixture=TestBed.createComponent(VehicleAxleDiagram);});
 it('renders two configured axles',()=>{fixture.componentInstance.axles=[{id:1,name:'Eje 1',type:'Direccional',positions:[]},{id:2,name:'Eje 2',type:'Tracción',positions:[]}];fixture.detectChanges();expect(fixture.nativeElement.querySelectorAll('.axle').length).toBe(2);});
 it('renders ten tractocamion positions and highlights selection',()=>{fixture.componentInstance.selectedId=4;fixture.componentInstance.axles=[{id:1,name:'Eje 1',type:'Direccional',positions:[p(1),p(2)]},{id:2,name:'Eje 2',type:'Tracción',positions:[p(3),p(4),p(5),p(6)]},{id:3,name:'Eje 3',type:'Tracción',positions:[p(7),p(8),p(9),p(10)]}];fixture.detectChanges();expect(fixture.nativeElement.querySelectorAll('.positions button').length).toBe(10);expect(fixture.nativeElement.querySelector('.positions button.selected b').textContent).toContain('P4');});
});
function p(id:number){return{id,code:`P${id}`,side:'Lado',tire:`LL-${id}`}}
