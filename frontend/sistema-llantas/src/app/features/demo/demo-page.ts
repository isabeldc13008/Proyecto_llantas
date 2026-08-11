import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';

type Row={primary:string;secondary:string;third:string;status:string;meta:string};
const rows:Record<string,Row[]>={
 vehiculos:[r('TJK-482','Tractocamión · Kenworth T680','Bogotá','Operativo','10 posiciones · 8 ocupadas'),r('WNP-193','Camión · Chevrolet FVR','Medellín','En taller','6 posiciones · 6 ocupadas'),r('LST-809','Bus · Mercedes-Benz O500','Cali','Operativo','8 posiciones · 8 ocupadas')],
 inventario:[r('Bogotá','1.248 llantas','Disponible','Saludable','72% disponibilidad'),r('Medellín','846 llantas','Disponible','Atención','18 en reparación'),r('Cali','621 llantas','Disponible','Saludable','9 en reencauche')],
 inspecciones:[r('INS-00842','TJK-482 · LL-000184','Hoy, 08:30','Completada','Exterior 12.4 · Centro 13.0 · Interior 12.1'),r('INS-00841','WNP-193 · LL-000327','Hoy, 07:15','Con alerta','Diferencia de desgaste: 3.2 mm'),r('INS-00840','LST-809 · LL-000401','Ayer, 16:40','Completada','Sin novedades')],
 alertas:[r('Desgaste crítico','LL-000327 · WNP-193','Profundidad: 2.1 mm','Crítica','Asignada a Carlos M.'),r('Inspección vencida','LL-000588 · TJK-482','Vencida hace 4 días','Alta','Sin responsable'),r('Rotación recomendada','LL-000184 · TJK-482','Diferencia: 2.8 mm','Media','Vence mañana')],
 programacion:[r('Inspección mensual','TJK-482','12 ago · 08:00','Programada','Técnico: Laura Ruiz'),r('Rotación preventiva','WNP-193','12 ago · 10:30','Pendiente','Prioridad alta'),r('Cambio de llanta','LST-809','13 ago · 07:00','Programada','Posición 5')],
 montajes:[r('MON-00291','LL-000184 → TJK-482','Posición 3','Completado','38.420 km · Laura Ruiz'),r('DES-00188','LL-000401 ← LST-809','Destino: reparación','Completado','Motivo: perforación lateral'),r('MON-00290','LL-000588 → WNP-193','Posición 6','Validando','Dimensión compatible')],
 movimientos:[r('MOV-019382','Montaje · LL-000184','Bogotá → TJK-482','Aplicado','Hoy, 08:42 · Laura Ruiz'),r('MOV-019381','Traslado · LL-000327','Medellín → Bogotá','En tránsito','Ayer, 15:20 · Carlos M.'),r('MOV-019380','Salida a reparación · LL-000401','Stock → Taller','Aplicado','Ayer, 11:05 · Andrés P.')],
 reparaciones:[r('REP-00127','LL-000401 · Perforación lateral','Taller Central Cali','En diagnóstico','$ 0 pendiente'),r('REP-00126','LL-000229 · Válvula','Servillantas Norte','Reparada','$ 85.000'),r('REP-00125','LL-000118 · Banda','Taller Aliado SAS','No reparable','$ 40.000')],
 reencauches:[r('REC-00082','LL-000332 · Reencauche #2','Banda XDA2','En proveedor','Entrega estimada: 15 ago'),r('REC-00081','LL-000211 · Reencauche #1','Banda KMAX D','Recibida','Profundidad recibida: 18 mm'),r('REC-00080','LL-000097 · Reencauche #3','Banda R297','Rechazado','Carcasa no apta')],
 disposicion:[r('DIS-00031','LL-000118','Daño estructural','Pendiente aprobación','Solicita: Líder taller Bogotá'),r('DIS-00030','LL-000076','Fin de vida útil','Aprobada','Evidencia adjunta · 06 ago'),r('DIS-00029','LL-000055','Accidente','Finalizada','Certificado: CF-8291')],
 historial:[r('LL-000184','Montaje en TJK-482 · Posición 3','Hoy, 08:42','Movimiento','Kilometraje: 38.420'),r('LL-000184','Inspección preventiva','05 ago, 09:12','Inspección','Profundidad promedio: 12.5 mm'),r('LL-000184','Traslado a Bogotá','28 jul, 14:20','Traslado','Desde centro Medellín')],
 carga:[r('Inventario_inicial_agosto.xlsx','20.000 filas detectadas','Previsualización','Listo para validar','Cargado por Isabel'),r('Actualizacion_centros.xlsx','418 filas','Procesado','398 exitosas · 20 rechazadas','Reporte disponible'),r('Llantas_nuevas_julio.xlsx','82 filas','Procesado','82 exitosas','Sin inconsistencias')],
 administracion:[r('Usuarios y accesos','48 usuarios activos','Seguridad','Configurado','4 roles · 17 permisos'),r('Catálogos de llantas','Marcas, referencias y dimensiones','Parametrización','Activo','126 registros'),r('Centros y talleres','3 centros · 7 talleres','Organización','Activo','Restricción por centro')],
 auditoria:[r('Isabel','Actualizó LL-000184','Llanta','Exitoso','Hoy, 09:41 · 192.168.1.24'),r('Laura Ruiz','Registró INS-00842','Inspección','Exitoso','Hoy, 08:30 · App web'),r('Carlos M.','Intentó montar LL-000327','Montaje','Bloqueado','Posición ocupada')],
 analitica:[r('Costo acumulado','$ 184,6 M','Últimos 12 meses','-8.4%','Frente al periodo anterior'),r('Vida útil promedio','86.420 km','Flota completa','+5.2%','Mejora interanual'),r('Alertas críticas','12 abiertas','Operación actual','Atención','4 vencen hoy')]
};
function r(primary:string,secondary:string,third:string,status:string,meta:string):Row{return{primary,secondary,third,status,meta};}
type Field={key:string;label:string;type?:'text'|'number'|'date'|'textarea'|'select';options?:string[]};
const fields:Record<string,Field[]>={
 vehiculos:[f('placa','Placa'),f('tipo','Tipo','select',['Tractocamión','Camión','Bus','Remolque']),f('marca','Marca'),f('modelo','Modelo'),f('centro','Centro','select',['Bogotá','Medellín','Cali']),f('posiciones','Número de posiciones','number')],
 inventario:[f('origen','Centro de origen','select',['Bogotá','Medellín','Cali']),f('destino','Centro de destino','select',['Bogotá','Medellín','Cali']),f('llanta','Código de llanta'),f('fecha','Fecha de traslado','date'),f('motivo','Motivo','textarea')],
 alertas:[f('tipo','Tipo de regla','select',['Desgaste crítico','Inspección vencida','Rotación','Presión fuera de rango']),f('limite','Valor límite','number'),f('prioridad','Prioridad','select',['Crítica','Alta','Media','Baja']),f('responsable','Responsable')],
 programacion:[f('actividad','Actividad','select',['Inspección','Rotación','Cambio','Mantenimiento']),f('vehiculo','Vehículo'),f('fecha','Fecha programada','date'),f('tecnico','Técnico'),f('prioridad','Prioridad','select',['Alta','Media','Baja'])],
 movimientos:[f('tipo','Tipo','select',['Traslado','Rotación','Ajuste autorizado','Salida de inventario']),f('llanta','Llanta'),f('origen','Origen'),f('destino','Destino'),f('motivo','Motivo','textarea')],
 reparaciones:[f('llanta','Llanta'),f('dano','Tipo de daño','select',['Perforación','Corte','Válvula','Banda','Carcasa']),f('taller','Taller o proveedor'),f('fecha','Fecha de envío','date'),f('diagnostico','Diagnóstico','textarea')],
 reencauches:[f('llanta','Llanta'),f('numero','Número de reencauche','number'),f('banda','Banda'),f('proveedor','Proveedor'),f('fecha','Fecha de envío','date')],
 disposicion:[f('llanta','Llanta'),f('motivo','Motivo','select',['Fin de vida útil','Daño estructural','Accidente','No reparable']),f('responsable','Responsable'),f('fecha','Fecha','date'),f('observaciones','Observaciones','textarea')],
 historial:[f('llanta','Código o serial de llanta'),f('desde','Desde','date'),f('hasta','Hasta','date')],
 carga:[f('archivo','Nombre del archivo Excel'),f('tipo','Tipo de carga','select',['Inventario inicial','Actualización','Llantas nuevas']),f('centro','Centro','select',['Todos','Bogotá','Medellín','Cali'])],
 administracion:[f('tipo','Tipo de registro','select',['Usuario','Rol','Centro','Taller','Marca','Referencia','Dimensión']),f('codigo','Código'),f('nombre','Nombre'),f('descripcion','Descripción','textarea')],
 auditoria:[f('usuario','Usuario'),f('entidad','Entidad'),f('desde','Desde','date'),f('hasta','Hasta','date')]
};
function f(key:string,label:string,type:Field['type']='text',options?:string[]):Field{return{key,label,type,options};}

@Component({selector:'app-demo-page',imports:[FormsModule],templateUrl:'./demo-page.html',styleUrls:['./demo-page.scss','./generic-form.scss']})
export class DemoPage{
 private readonly route=inject(ActivatedRoute);
 readonly data=this.route.snapshot.data as {title:string;eyebrow:string;description:string;action:string;kind:string}; readonly items=rows[this.data.kind]??[]; readonly formFields=fields[this.data.kind]??[f('detalle','Detalle'),f('observaciones','Observaciones','textarea')]; toast=''; showForm=false;formData:Record<string,string|number>={};custom=signal<Row[]>(this.read());
 allItems(){return[...this.custom(),...this.items]}
 act(message?:string){if(message){this.toast=message;setTimeout(()=>this.toast='',2800);return}if(this.data.kind==='analitica'){this.toast='Reporte de demostración generado correctamente.';setTimeout(()=>this.toast='',2800);return}this.formData={};this.showForm=true;}
 save(){const values=Object.values(this.formData).filter(Boolean).map(String);const row:Row={primary:values[0]||`REG-${Date.now().toString().slice(-5)}`,secondary:values[1]||this.data.action,third:values[2]||'GLLD',status:'Registrado',meta:`Creado ahora · Modo demostración`};const all=[row,...this.custom()];this.custom.set(all);localStorage.setItem(`glld_${this.data.kind}`,JSON.stringify(all));this.showForm=false;this.toast='Registro guardado en la demostración.';setTimeout(()=>this.toast='',2800);}
 private read():Row[]{try{return JSON.parse(localStorage.getItem(`glld_${this.data.kind}`)??'[]')}catch{return[]}}
}
