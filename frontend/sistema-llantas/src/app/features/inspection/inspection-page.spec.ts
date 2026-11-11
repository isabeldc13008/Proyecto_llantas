import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { InspectionPage } from './inspection-page';
import { provideRouter } from '@angular/router';

describe('InspectionPage vehicle search', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InspectionPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('recupera alertas e inconsistencias persistidas al recargar y mantiene la configuración real',async()=>{
    spyOn(InspectionPage.prototype,'ngOnInit').and.resolveTo();const f=TestBed.createComponent(InspectionPage);const p=f.componentInstance;
    const loading=p.resumeInspection('persistida');
    http.expectOne('/api/inspecciones/persistida').flush({id:'persistida',vehiculoId:'v',centroId:'c',kilometraje:100,estado:'Borrador',detalles:[{posicionId:'p',llantaId:'t',exterior:0,centro:0,interior:0,condicionId:'c',recomendacionId:'r'}],inconsistencias:[{id:'inc',posicionId:'p',llantaEncontradaId:'otra',identificadorEncontrado:'OTRA',estado:'PendienteAutorizacion'}],alertasPosiciones:[{id:'a',posicionId:'p',llantaId:'t',tipo:'Regla real',descripcion:'Hallazgo persistido',estado:'ABIERTA'}]});
    await Promise.resolve();
    http.expectOne('/api/inspecciones/contexto/v').flush({vehiculoId:'v',centroId:'c',numeroInterno:'1',placa:'P',tipo:'Camión',centroNombre:'C',ejes:[{id:'e',numero:4,nombre:'Eje real',tipo:'Auxiliar',posiciones:[{id:'p',codigo:'P4',lado:'Derecho',orden:3,llanta:{id:'t',codigo:'LL',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});await loading;f.detectChanges();
    expect(p.completed()).toBe(1);expect(p.alertCount()).toBe(1);expect(p.diagramAxles()[0].type).toBe('Auxiliar');expect(p.diagramAxles()[0].positions[0].inspectionStatus).toBe('active-alert');
    const button=f.nativeElement.querySelector('[data-inspection-status="active-alert"]');expect(button.getAttribute('aria-label')).toContain('Hallazgo persistido');expect(button.getAttribute('aria-label')).toContain('OTRA');
    p.backToScheme();expect(p.diagramAxles()[0].positions[0].inspectionStatus).toBe('active-alert');
  });

  it('no cuenta una inspección parcial ni convierte una alerta gestionada en roja',()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());p.positions.set([position('p')]);p.selectedId.set('p');
    p.applyPersisted({detalles:[{posicionId:'p',llantaId:'t',exterior:null,centro:3,interior:3,condicionId:'c',recomendacionId:'r'}],inconsistencias:[],alertasPosiciones:[{posicionId:'p',estado:'GESTIONADA'}]} as any,'p');
    expect(p.completed()).toBe(0);expect(p.alertCount()).toBe(0);expect(p.positionStatusText(p.selected())).toContain('Pendiente');
  });

  it('guarda reencauche sin Tipo y conserva marca y vida al consultar nuevamente',async()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());(p as any).inspectionId='i';p.physicalCenterId='fisico';
    const tire={id:'n',codigo:'N',estado:'MONTADA',marca:'Original',referencia:'R',dimension:'D',numeroReencauches:2,vida:3,esReencauchada:true,marcaReencaucheId:'mr',marcaReencauche:'Marca R'};
    const context={vehiculoId:'v',numeroInterno:'T1',placa:'P',tipo:'Camión',centroId:'adscripcion',centroNombre:'Origen',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:null as typeof tire|null}]}]};
    (p as any).mapContext(context);p.openAssignment();p.assignmentMode.set('new');p.foundCatalogs.set({marcas:[],referencias:[],dimensiones:[],tipos:[]});
    Object.assign(p.newTire,{serial:'S',marcaId:'m',referenciaId:'r',dimensionId:'d',profundidadInicial:12,esReencauchada:true,marcaReencaucheId:'mr',vida:3});p.physicalDiscrepancy=true;p.assignmentReason='Observada';
    const saving=p.assignTire();await Promise.resolve();const post=http.expectOne('/api/inspecciones/i/llantas-encontradas');
    expect(post.request.body.tipoLlantaId).toBeUndefined();expect(post.request.body.centroId).toBe('fisico');expect(post.request.body.marcaReencaucheId).toBe('mr');expect(post.request.body.vida).toBe(3);
    context.ejes[0].posiciones[0].llanta=tire;post.flush(context);await saving;
    expect(p.retreadDescription(p.selected())).toContain('Marca R · Vida 3');
    p.vehicles.set([{id:'v',internal:'T1',plate:'P',type:'Camión',centerId:'adscripcion',centerCode:'A',centerName:'Origen',regional:''}]);
    const opening=p.openInspection({id:'i',fecha:'2026-11-05',placa:'P',numeroInterno:'T1',centro:'Físico',tecnico:'T',kilometraje:100,inspeccionadas:0,novedades:0,alertas:0,estado:'Borrador'});
    http.expectOne('/api/inspecciones/contexto/v').flush(context);http.expectOne('/api/inspecciones/i').flush({id:'i',centroId:'fisico',kilometraje:100,estado:'Borrador',detalles:[],inconsistencias:[]});await opening;
    expect(p.selected().marcaReencaucheId).toBe('mr');expect(p.selected().vida).toBe(3);expect(p.physicalCenterId).toBe('fisico');expect(p.context()?.centroId).toBe('adscripcion');p.ngOnDestroy();
  });

  it('distingue vida 1 y marca histórica desconocida sin inventar valores',()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());
    expect(p.retreadDescription({numeroReencauches:0})).toBe('No reencauchada · Vida 1');
    expect(p.retreadDescription({numeroReencauches:1,marcaReencauche:null})).toContain('Sin información registrada · Vida 2');
    expect(p.retreadDescription({})).toBe('Reencauche: sin información');
  });

  function position(id:string){return {id,code:id,side:'I',tire:'LL-'+id,brand:'M',reference:'R',dimension:'D',state:'normal' as const,outer:0,center:1,inner:2,condition:'c',cause:'',recommendation:'r',notes:'',saved:false,identification:'confirmed' as const};}

  it('filtra causas y recomendaciones y limpia las selecciones dependientes',()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());
    p.positions.set([position('p')]);p.selectedId.set('p');
    p.options.set({condiciones:[{id:'cond',codigo:'C',nombre:'Daño'}],causas:[{id:'cause',codigo:'CA',nombre:'Causa'}],recomendaciones:[{id:'rec',codigo:'R',nombre:'Recomendación'}],condicionCausas:[{origenId:'cond',destinoId:'cause'}],causaRecomendaciones:[{origenId:'cause',destinoId:'rec'}]});
    p.conditionChanged('cond');expect(p.filteredCauses().map(x=>x.id)).toEqual(['cause']);expect(p.selected().cause).toBe('');
    p.causeChanged('cause');expect(p.filteredRecommendations().map(x=>x.id)).toEqual(['rec']);expect(p.selected().recommendation).toBe('');
    p.recommendationChanged('rec');p.conditionChanged('cond');expect(p.selected().cause).toBe('');expect(p.selected().recommendation).toBe('');
  });

  it('guarda una vez, vuelve al esquema y destaca la siguiente pendiente conservando su borrador',async()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());(p as any).inspectionId='i';
    const first=position('p1'),second={...position('p2'),notes:'Pendiente local'};
    p.positions.set([first,second]);p.selectedId.set('p1');p.stage.set('tires');p.editingPosition.set(true);
    const saving=p.savePosition();await p.savePosition();await Promise.resolve();
    const req=http.expectOne('/api/inspecciones/i/posiciones/p1');expect(req.request.body.profundidadExterior).toBe(0);
    req.flush({detalles:[{posicionId:'p1',llantaId:'t',exterior:0,centro:1,interior:2,condicionId:'c',recomendacionId:'r'}],inconsistencias:[],alertasPosiciones:[]});await saving;
    expect(first.saved).toBeTrue();expect(p.stage()).toBe('tires');expect(p.editingPosition()).toBeFalse();expect(p.selectedId()).toBe('p2');expect(p.selected().notes).toBe('Pendiente local');
    p.previous();expect(p.selectedId()).toBe('p1');expect(p.selected().saved).toBeTrue();p.ngOnDestroy();
  });

  it('conserva las lecturas pendientes al volver al esquema y al cambiar de posición',()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());p.positions.set([position('p1'),position('p2')]);p.selectedId.set('p1');
    p.selected().notes='Sin guardar';p.selected().saved=true;p.markPositionPending();p.backToScheme();p.nextPosition();p.previous();
    expect(p.selected().notes).toBe('Sin guardar');expect(p.selected().outer).toBe(0);expect(p.selected().saved).toBeFalse();
  });

  it('mantiene la posición y sus datos si falla el guardado',async()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());(p as any).inspectionId='i';p.positions.set([position('p')]);p.selectedId.set('p');p.editingPosition.set(true);
    const saving=p.savePosition();await Promise.resolve();http.expectOne('/api/inspecciones/i/posiciones/p').flush({message:'Conflicto'},{status:409,statusText:'Conflict'});await saving;
    expect(p.editingPosition()).toBeTrue();expect(p.selected().saved).toBeFalse();expect(p.selected().outer).toBe(0);expect(p.savingPosition()).toBeFalse();
  });

  it('verifica una posición vacía sin crear llantas ni montajes',async()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());(p as any).inspectionId='i';p.positions.set([position('p')]);p.selectedId.set('p');
    const checking=p.verifyVacancy();await Promise.resolve();http.expectOne('/api/inspecciones/i/posiciones/p/asignacion').flush({vacanteVerificada:false,asignaciones:[{codigo:'LL-p'}]});await checking;
    expect(p.vacancyMessage()).toContain('LL-p');expect(p.vacancyMessage()).toContain('no se modificó');http.expectNone(r=>r.method!=='GET');
  });

  it('abre la búsqueda primero y no permite registrar antes de buscar',async()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());p.openAssignment();expect(p.assignmentMode()).toBe('existing');await p.startNewTire();
    expect(p.assignmentMode()).toBe('existing');http.expectNone(r=>true);
  });

  it('registra el centro físico sin alterar el de adscripción y evita iniciar dos veces',async()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());p.physicalCenterId='fisico';p.mileage=150;p.selectedVehicleId.set('viajero');
    p.context.set({vehiculoId:'viajero',numeroInterno:'T1',placa:'ABC',tipo:'Camión',centroId:'adscripcion',centroNombre:'Origen',ejes:[]});
    const starting=p.startInspection();await p.startInspection();const req=http.expectOne('/api/inspecciones');
    expect(req.request.body.centroId).toBe('fisico');expect(req.request.body.vehiculoId).toBe('viajero');req.flush({id:'i'});await starting;
    expect(p.context()?.centroId).toBe('adscripcion');expect(p.stage()).toBe('tires');
  });

  it('no permite seleccionar una llanta registrada no disponible',()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());p.openAssignment();
    p.selectPhysicalTire({id:'t',codigo:'T',serial:'S',marca:'M',referencia:'R',dimension:'D',estado:'Montada',disponible:false});
    expect(p.assignmentTireId).toBe('');expect(p.assignmentReady()).toBeFalse();
  });

  it('conserva un único acceso a corrección y navegación accesible',()=>{
    spyOn(InspectionPage.prototype,'ngOnInit').and.resolveTo();const f=TestBed.createComponent(InspectionPage);const p=f.componentInstance;
    p.positions.set([position('p')]);p.selectedId.set('p');p.stage.set('tires');f.detectChanges();
    const text=f.nativeElement.textContent;expect(text).not.toContain('Corregir: físicamente');expect(f.nativeElement.querySelectorAll('button.mismatch').length).toBe(1);
    expect(text).toContain('Volver al esquema');expect(text).toContain('Anterior');
  });

  it('exige marca y vida solo para reencauchadas y limpia los datos al elegir No',()=>{
    const p=TestBed.runInInjectionContext(()=>new InspectionPage());p.assignmentMode.set('new');p.physicalDiscrepancy=true;p.assignmentReason='Corrección';
    p.foundCatalogs.set({marcas:[],referencias:[],dimensiones:[],tipos:[]});
    Object.assign(p.newTire,{serial:'S',marcaId:'m',referenciaId:'r',dimensionId:'d',profundidadInicial:0});
    expect(p.assignmentReady()).toBeTrue();p.newTire.esReencauchada=true;
    expect(p.newTireValidation()).toContain('marca de reencauche');p.newTire.marcaReencaucheId='m';
    for(const vida of [null,0,1,2.5]){p.newTire.vida=vida;expect(p.newTireValidation()).toContain('vida de reencauche');}
    p.newTire.vida=2;expect(p.assignmentReady()).toBeTrue();
    p.newTire.esReencauchada=false;p.retreadChanged();expect(p.newTire.marcaReencaucheId).toBe('');expect(p.newTire.vida).toBeNull();expect(p.assignmentReady()).toBeTrue();
  });

  it('muestra validación de reencauche dentro del modal y no muestra Tipo',async()=>{
    spyOn(InspectionPage.prototype,'ngOnInit').and.resolveTo();const f=TestBed.createComponent(InspectionPage);const p=f.componentInstance;
    p.positions.set([{id:'p',code:'P1',side:'I',tire:'Sin llanta',brand:'',reference:'',dimension:'',state:'empty',outer:null,center:null,inner:null,condition:'',cause:'',recommendation:'',notes:'',saved:false,identification:'vacant'}]);p.selectedId.set('p');
    p.assignmentOpen.set(true);p.assignmentMode.set('new');p.physicalDiscrepancy=true;p.assignmentReason='Observada';
    p.foundCatalogs.set({marcas:[],referencias:[],dimensiones:[],tipos:[]});Object.assign(p.newTire,{serial:'S',marcaId:'m',referenciaId:'r',dimensionId:'d',esReencauchada:true,profundidadInicial:12});
    await p.assignTire();f.detectChanges();const modal=f.nativeElement.querySelector('.physical-modal');
    expect(modal.querySelector('[role="alert"]').textContent).toContain('marca de reencauche');
    expect([...modal.querySelectorAll('label')].some((l:any)=>l.textContent.trim().startsWith('Tipo'))).toBeFalse();
    http.expectNone('/api/inspecciones');
  });

  it('filters vehicles by internal number, plate, type, center and without accents', async () => {
    const fixture = TestBed.createComponent(InspectionPage);
    fixture.detectChanges();

    http.expectOne('/api/inspecciones/vehiculos').flush([
      { id: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroCodigo: 'BOG', centroNombre: 'Bogotá Norte' },
      { id: '2', numeroInterno: 'CAM-202', placa: 'XYZ987', tipo: 'Camión', centroCodigo: 'MED', centroNombre: 'Medellín' },
    ]);
    http.expectOne('/api/inspecciones/opciones').flush({ condiciones: [], causas: [], recomendaciones: [] });
    http.expectOne('/api/inspecciones/resumen').flush({ pendientesHoy: 0, realizadasHoy: 0, conNovedad: 0, conAlerta: 0 });
    http.expectOne('/api/inspecciones/historial').flush([]);http.expectOne('/api/inspecciones/centros-fisicos').flush([]);
    await fixture.whenStable();
    expect(fixture.componentInstance.selectedVehicleId()).toBeFalsy();
    void fixture.componentInstance.chooseVehicle('1');
    http.expectOne('/api/inspecciones/contexto/1').flush({ vehiculoId: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroNombre: 'Bogotá Norte', ejes: [] });
    await fixture.whenStable();

    fixture.componentInstance.vehicleSearch.set('camion med');
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredVehicles().map(vehicle => vehicle.id)).toEqual(['2']);
  });

  it('keeps the active vehicle unchanged when a search has no results', async () => {
    const fixture = TestBed.createComponent(InspectionPage);
    fixture.detectChanges();

    http.expectOne('/api/inspecciones/vehiculos').flush([
      { id: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroCodigo: 'BOG', centroNombre: 'Bogotá' },
    ]);
    http.expectOne('/api/inspecciones/opciones').flush({ condiciones: [], causas: [], recomendaciones: [] });
    http.expectOne('/api/inspecciones/resumen').flush({ pendientesHoy: 0, realizadasHoy: 0, conNovedad: 0, conAlerta: 0 });
    http.expectOne('/api/inspecciones/historial').flush([]);http.expectOne('/api/inspecciones/centros-fisicos').flush([]);
    await fixture.whenStable();
    expect(fixture.componentInstance.selectedVehicleId()).toBeFalsy();
    void fixture.componentInstance.chooseVehicle('1');
    http.expectOne('/api/inspecciones/contexto/1').flush({ vehiculoId: '1', numeroInterno: 'BUS-101', placa: 'ABC123', tipo: 'Bus', centroNombre: 'Bogotá', ejes: [] });
    await fixture.whenStable();

    fixture.componentInstance.vehicleSearch.set('no existe');
    fixture.detectChanges();

    expect(fixture.componentInstance.filteredVehicles()).toEqual([]);
    expect(fixture.componentInstance.selectedVehicleId()).toBe('1');
    expect(fixture.componentInstance.stage()).toBe('summary');
  });

  it('blocks operational mounting without a physical discrepancy',async()=>{const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.assignmentTireId='new';page.assignmentReason='Instalar nueva';await page.assignTire();http.expectNone(r=>r.method==='POST');expect(page.assignmentMessage()).toContain('Programación');});
  it('sends physical identity and expected previous tire when correcting',async()=>{const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.mileage=1000;page.physicalCenterId='c';page.selectedVehicleId.set('v');page.physicalDiscrepancy=true;page.physicalIdentifier='SER-Y';page.assignmentTireId='Y';page.assignmentReason='Encontrada físicamente';page.context.set({vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:{id:'X',codigo:'X',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});page.positions.set([{id:'p',code:'P1',side:'I',tire:'X',brand:'M',reference:'R',dimension:'D',state:'normal',outer:null,center:null,inner:null,condition:'',cause:'',recommendation:'',notes:'',saved:false,identification:'confirmed'}]);page.selectedId.set('p');const pending=page.assignTire();http.expectOne('/api/inspecciones').flush({id:'i1'});await Promise.resolve();await Promise.resolve();await Promise.resolve();await Promise.resolve();const req=http.expectOne('/api/inspecciones/i1/posiciones/p/asignar');expect(req.request.body).toEqual({llantaId:'Y',motivo:'Encontrada físicamente',discrepanciaFisica:true,identificadorFisico:'SER-Y',llantaAnteriorId:'X'});req.flush({...page.context(),ejes:[]});await pending;});

  it('precarga odómetro y bloquea kilometraje inferior',async()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());const pending=page.chooseVehicle('v');
    http.expectOne('/api/inspecciones/contexto/v').flush({vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',kilometraje:100000,ejes:[]});await pending;
    expect(page.mileage).toBe(100000);expect(page.mileageValid()).toBeTrue();page.mileage=99999;expect(page.mileageValid()).toBeFalse();
  });
  it('permite cerrar mensajes, autocierra éxito y conserva errores',fakeAsync(()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.notify('Guardada','success');tick(4499);expect(page.message()).toBe('Guardada');tick(1);expect(page.message()).toBe('');
    page.notify('No guardada');tick(10000);expect(page.message()).toBe('No guardada');page.clearMessage();expect(page.message()).toBe('');
    page.notify('Anterior');page.changeStage('summary');expect(page.message()).toBe('');page.ngOnDestroy();
  }));
  it('el botón del aviso cierra sin bloquear las acciones',()=>{
    spyOn(InspectionPage.prototype,'ngOnInit').and.resolveTo();const f=TestBed.createComponent(InspectionPage);f.componentInstance.notify('Error');f.detectChanges();
    const button=f.nativeElement.querySelector('.toast button');expect(button.getAttribute('aria-label')).toBe('Cerrar mensaje');button.click();f.detectChanges();expect(f.nativeElement.querySelector('.toast')).toBeNull();
  });
  it('registra nueva por endpoint limitado y continúa leyendo la misma posición',async()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.mileage=1000;page.physicalCenterId='c';page.selectedVehicleId.set('v');
    const context={vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:null}]}]};
    page.context.set(context);page.positions.set([{id:'p',code:'P1',side:'I',tire:'Sin llanta',brand:'',reference:'',dimension:'',state:'empty',outer:null,center:null,inner:null,condition:'',cause:'',recommendation:'',notes:'',saved:false,identification:'vacant'}]);page.selectedId.set('p');
    page.openAssignment();page.assignmentMode.set('new');page.foundCatalogs.set({marcas:[],referencias:[],dimensiones:[],tipos:[]});page.newTire={codigo:'N',serial:'S',marcaId:'m',referenciaId:'r',dimensionId:'d',esReencauchada:false,marcaReencaucheId:'',vida:null,profundidadInicial:12};page.physicalDiscrepancy=true;page.assignmentReason='Observada';
    const pending=page.assignTire();http.expectOne('/api/inspecciones').flush({id:'i'});await Promise.resolve();await Promise.resolve();await Promise.resolve();await Promise.resolve();
    const req=http.expectOne('/api/inspecciones/i/llantas-encontradas');expect(req.request.body.tipoLlantaId).toBeUndefined();expect(req.request.body.marcaReencaucheId).toBeUndefined();expect(req.request.body.vida).toBeUndefined();expect(req.request.body.centroId).toBe('c');expect(req.request.body.llantaAnteriorId).toBeNull();http.expectNone('/api/llantas');
    req.flush({...context,ejes:[{...context.ejes[0],posiciones:[{...context.ejes[0].posiciones[0],llanta:{id:'n',codigo:'N',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});await pending;
    expect(page.assignmentOpen()).toBeFalse();expect(page.selected().id).toBe('p');expect(page.selected().tire).toBe('N');expect(page.selected().state).toBe('normal');expect(page.selected().saved).toBeFalse();
    Object.assign(page.selected(),{outer:11,center:11,inner:11,condition:'cond',recommendation:'rec'});const save=page.savePosition();await Promise.resolve();await Promise.resolve();await Promise.resolve();await Promise.resolve();const put=http.expectOne('/api/inspecciones/i/posiciones/p');expect(put.request.body.profundidadCentro).toBe(11);put.flush({detalles:[{posicionId:'p',llantaId:'n',exterior:11,centro:11,interior:11,condicionId:'cond',recomendacionId:'rec'}],inconsistencias:[],alertasPosiciones:[]});await save;expect(page.selected().saved).toBeTrue();page.ngOnDestroy();
  });
  it('una corrección regularizada no vuelve a quedar pendiente al abrir borrador',async()=>{
    const page=TestBed.runInInjectionContext(()=>new InspectionPage());page.vehicles.set([{id:'v',internal:'1',plate:'P',type:'Camión',centerId:'c',centerCode:'C',centerName:'Centro',regional:''}]);
    const pending=page.openInspection({id:'i',fecha:'2026-09-29',placa:'P',numeroInterno:'1',centro:'C',tecnico:'T',kilometraje:1000,inspeccionadas:0,novedades:0,alertas:0,estado:'Borrador'});
    http.expectOne('/api/inspecciones/contexto/v').flush({vehiculoId:'v',numeroInterno:'1',placa:'P',tipo:'Camión',centroId:'c',centroNombre:'C',ejes:[{id:'e',numero:1,nombre:'E',posiciones:[{id:'p',codigo:'P1',lado:'I',orden:1,llanta:{id:'n',codigo:'N',estado:'MONTADA',marca:'M',referencia:'R',dimension:'D'}}]}]});
    http.expectOne('/api/inspecciones/i').flush({id:'i',vehiculoId:'v',kilometraje:1000,estado:'Borrador',detalles:[],inconsistencias:[{id:'x',posicionId:'p',llantaEncontradaId:'n',identificadorEncontrado:'N',estado:'Regularizada'}]});await pending;
    expect(page.selected().state).toBe('normal');expect(page.selected().identification).toBe('confirmed');
  });
});


