import {provideHttpClient} from '@angular/common/http';
import {HttpTestingController,provideHttpClientTesting} from '@angular/common/http/testing';
import {TestBed} from '@angular/core/testing';
import {VehiclesApi} from './vehicles-api';

describe('VehiclesApi',()=>{
 let api:VehiclesApi;let http:HttpTestingController;
 beforeEach(()=>{TestBed.configureTestingModule({providers:[provideHttpClient(),provideHttpClientTesting()]});api=TestBed.inject(VehiclesApi);http=TestBed.inject(HttpTestingController)});
 afterEach(()=>http.verify());
 it('persiste una configuración dinámica completa',()=>{const input={codigo:'TRACTO-6X4',nombre:'Tractocamión 6x4',tipoVehiculo:'Tractocamión',ejes:[{orden:1,nombre:'Eje 1',tipoEje:'Direccional',posiciones:[{codigo:'P1',lado:'Izquierda',ubicacion:'Externa',orden:1}]},{orden:2,nombre:'Eje 2',tipoEje:'Tracción',posiciones:[{codigo:'P2',lado:'Derecha',ubicacion:'Interna',orden:1}]}]};api.createConfiguration(input).subscribe();const request=http.expectOne('/api/vehiculos/configuraciones');expect(request.request.method).toBe('POST');expect(request.request.body).toEqual(input);request.flush({...input,id:'1',activo:true})});
 it('envía la configuración homologada al crear el vehículo',()=>{const input={numeroInterno:'100',placa:'ABC123',tipo:'Tractocamión',centroId:'c1',configuracionVehiculoId:'cfg1',kilometraje:1000,estado:'Activo'};api.create(input).subscribe();const request=http.expectOne('/api/vehiculos');expect(request.request.body.configuracionVehiculoId).toBe('cfg1');request.flush({})});
});
