import { AuthService } from '../../core/auth/auth.service';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CatalogItem } from '../../core/models/api.models';
import { CatalogsApi } from '../../core/services/catalogs-api';

interface CatalogType {key:string;name:string;description:string}
interface UserRow{id:string;username:string;nombre:string;activo:boolean;rolId:string;rol:string;rolCodigo:string;centroIds:string[];centros:string[];accesoGlobal:boolean}
interface ManagedRole{id:string;codigo:string;nombre:string;activo:boolean;usuarios:number;usuariosActivos:number;permisos?:string[]}
interface PermissionRow{codigo:string;nombre:string;activo:boolean}
interface PermissionGroup{key:string;label:string;permissions:PermissionRow[]}
interface UserImportRow{fila:number;correo:string;nombre:string;rol:string;centro:string;activo:string}
interface UserImportError{fila:number;campo:string;error:string;valorRecibido:string}
interface UserImportResult{filas:UserImportRow[];errores:UserImportError[];total:number;validas:number;conError:number;puedeImportar:boolean}
interface RoleRow{id:string;codigo:string;nombre:string;permisos:string[]}
@Component({selector:'app-catalog-admin',imports:[FormsModule],templateUrl:'./catalog-admin.html',styleUrl:'./catalog-admin.scss'})
export class CatalogAdmin implements OnInit{
 readonly auth=inject(AuthService);
 private readonly http=inject(HttpClient);
 private readonly catalogsApi=inject(CatalogsApi);
 readonly types:CatalogType[]=[{key:'usuarios',name:'Usuarios',description:'Cuentas, roles y accesos efectivos'},{key:'roles',name:'Roles',description:'Permisos compartidos por rol'},{key:'marcas',name:'Marcas',description:'Fabricantes de llantas'},{key:'referencias',name:'Referencias',description:'Líneas y modelos asociados a una marca'},{key:'dimensiones',name:'Dimensiones',description:'Medidas homologadas'},{key:'tipos-llanta',name:'Tipos de llanta',description:'Clasificación operativa'},{key:'estados-llanta',name:'Estados',description:'Estados del ciclo de vida'},{key:'regionales',name:'Regionales',description:'Agrupación territorial de centros'},{key:'centros',name:'Centros',description:'Centros operativos por regional'}];
 selected=signal(this.types[0]);records=signal<CatalogItem[]>([]);brands=signal<CatalogItem[]>([]);centers=signal<CatalogItem[]>([]);users=signal<UserRow[]>([]);roles=signal<RoleRow[]>([]);loading=signal(true);error=signal('');showForm=false;codigo='';nombre='';parentId='';search='';message='';username='';password='';roleId='';editingId='';active=true;centerIds:string[]=[];
 managedRoles=signal<ManagedRole[]>([]);permissionCatalog=signal<PermissionRow[]>([]);
 permissionCodes:string[]=[];permissionSearch='';roleWasActive=true;roleSaving=false;
 showImport=false;importBusy=false;importCsv='';importPassword='';importError='';importResult:UserImportResult|null=null;
 ngOnInit(){void this.load()}
 async choose(type:CatalogType){this.selected.set(type);this.search='';this.showForm=false;await this.load()}
 async load(){this.loading.set(true);this.error.set('');try{if(this.selected().key==='roles'){const [roles,permissions]=await Promise.all([firstValueFrom(this.http.get<ManagedRole[]>('/api/roles')),firstValueFrom(this.http.get<PermissionRow[]>('/api/roles/permisos'))]);this.managedRoles.set(roles);this.permissionCatalog.set(permissions);return}if(this.selected().key==='usuarios'){const [users,roles,centers]=await Promise.all([firstValueFrom(this.http.get<UserRow[]>('/api/usuarios')),firstValueFrom(this.http.get<RoleRow[]>('/api/usuarios/roles')),firstValueFrom(this.catalogsApi.all('centros',true))]);this.users.set(users);this.roles.set(roles);this.centers.set(centers);return}const all=await firstValueFrom(this.catalogsApi.all(this.selected().key));const term=this.search.trim().toLocaleLowerCase('es');this.records.set(term?all.filter(x=>`${x.codigo} ${x.nombre}`.toLocaleLowerCase('es').includes(term)):all);const parentType=this.selected().key==='referencias'?'marcas':this.selected().key==='centros'?'regionales':'';this.brands.set(parentType?await firstValueFrom(this.catalogsApi.all(parentType,true)):[])}catch(error:any){this.error.set(error?.userMessage??'No fue posible cargar los datos desde la base de datos.')}finally{this.loading.set(false)}}
 open(){this.error.set('');this.permissionCodes=[];this.permissionSearch='';this.roleWasActive=true;this.codigo='';this.nombre='';this.parentId='';this.username='';this.password='';this.roleId='';this.editingId='';this.active=true;this.centerIds=[];this.showForm=true}
 edit(record:CatalogItem){this.editingId=record.id;this.codigo=record.codigo;this.nombre=record.nombre;this.parentId=record.padreId??'';this.showForm=true}
 async save(){if(!this.codigo.trim()||!this.nombre.trim()||(['referencias','centros'].includes(this.selected().key)&&!this.parentId))return;this.error.set('');try{const body={codigo:this.codigo.trim(),nombre:this.nombre.trim(),padreId:this.parentId||null};const saved=await firstValueFrom(this.editingId?this.http.put<CatalogItem>(`/api/catalogos/${this.selected().key}/${this.editingId}`,body):this.http.post<CatalogItem>(`/api/catalogos/${this.selected().key}`,body));this.records.update(all=>this.editingId?all.map(item=>item.id===saved.id?saved:item):[saved,...all]);this.showForm=false;this.message=`${saved.nombre} fue guardado en la base de datos.`;setTimeout(()=>this.message='',2500)}catch(error:any){this.error.set(error?.userMessage??'No fue posible guardar el parámetro.')}}
 async toggle(record:CatalogItem){const activo=!record.activo;this.error.set('');try{await firstValueFrom(this.http.patch(`/api/catalogos/${this.selected().key}/${record.id}/estado`,{activo}));this.records.update(all=>all.map(item=>item.id===record.id?{...item,activo}:item))}catch(error:any){this.error.set(error?.userMessage??'No fue posible cambiar el estado.')}}
 editUser(user:UserRow){this.editingId=user.id;this.username=user.username;this.nombre=user.nombre;this.roleId=user.rolId;this.active=user.activo;this.password='';this.centerIds=[...user.centroIds];this.showForm=true}
 async saveUser(){try{const body={nombre:this.nombre,rolId:this.roleId,activo:this.active,password:this.password||null,centroIds:this.centerIds};if(this.editingId)await firstValueFrom(this.http.put(`/api/usuarios/${this.editingId}`,body));else await firstValueFrom(this.http.post('/api/usuarios',{username:this.username,...body,password:this.password}));this.showForm=false;await this.load()}catch(error:any){this.error.set(error?.userMessage??'No fue posible guardar el usuario.')}}
 toggleCenter(id:string,checked:boolean){this.centerIds=checked?[...new Set([...this.centerIds,id])]:this.centerIds.filter(x=>x!==id)}
 modules(roleId:string){const role=this.roles().find(x=>x.id===roleId);if(!role)return'';return role.permisos.join(' · ')}

 async editRole(role:ManagedRole){this.error.set('');try{
  const detail=await firstValueFrom(this.http.get<ManagedRole>(`/api/roles/${role.id}`));
  this.editingId=detail.id;this.codigo=detail.codigo;this.nombre=detail.nombre;this.active=detail.activo;
  this.roleWasActive=detail.activo;this.permissionCodes=[...(detail.permisos??[])];this.permissionSearch='';this.showForm=true;
 }catch(error:any){this.error.set(error?.error?.message??error?.userMessage??'No fue posible cargar el rol.')}}
 async confirmRoleDeactivation(id:string):Promise<number|null>{
  const role=await firstValueFrom(this.http.get<ManagedRole>(`/api/roles/${id}`));
  return window.confirm(`Desactivar ${role.nombre} impedirá el acceso a ${role.usuariosActivos} usuarios activos. ¿Continuar?`)?role.usuariosActivos:null;
 }
 async saveRole(){if(this.roleSaving)return;this.error.set('');
  if(!this.nombre.trim()||(!this.editingId&&!this.codigo.trim())){this.error.set('Código y nombre son obligatorios.');return}
  this.roleSaving=true;try{
   let usuariosActivosConfirmados:number|null=null;
   if(this.editingId&&this.roleWasActive&&!this.active){usuariosActivosConfirmados=await this.confirmRoleDeactivation(this.editingId);if(usuariosActivosConfirmados===null)return}
   const body={nombre:this.nombre.trim(),permisos:this.permissionCodes,activo:this.active,usuariosActivosConfirmados};
   if(this.editingId)await firstValueFrom(this.http.put(`/api/roles/${this.editingId}`,body));
   else await firstValueFrom(this.http.post('/api/roles',{...body,codigo:this.codigo.trim().toUpperCase()}));
   this.showForm=false;await this.load();
  }catch(error:any){this.error.set(error?.error?.message??error?.userMessage??'No fue posible guardar el rol.')}finally{this.roleSaving=false}
 }
 async toggleRole(role:ManagedRole){if(this.roleSaving)return;this.roleSaving=true;this.error.set('');try{
  const usuariosActivosConfirmados=role.activo?await this.confirmRoleDeactivation(role.id):null;
  if(role.activo&&usuariosActivosConfirmados===null)return;
  await firstValueFrom(this.http.patch(`/api/roles/${role.id}/estado`,{activo:!role.activo,usuariosActivosConfirmados}));await this.load();
 }catch(error:any){this.error.set(error?.error?.message??error?.userMessage??'No fue posible cambiar el estado del rol.')}finally{this.roleSaving=false}}
 permissionGroups():PermissionGroup[]{
  const labels:Record<string,string>={resumen:'Resumen',actividades:'Mis actividades',llantas:'Llantas',vehiculos:'Vehículos',inventario:'Inventario',inspecciones:'Inspecciones',alertas:'Alertas',programacion:'Programación',montajes:'Montajes',movimientos:'Movimientos',reparaciones:'Reparaciones',reencauches:'Reencauches',disposicion:'Disposición final',historial:'Historial',carga_masiva:'Carga masiva',analitica:'Analítica',administracion:'Administración',auditoria:'Auditoría',centros:'Centros',operaciones:'Operaciones (compartidos)',servicios_llanta:'Servicios de llantas (compartidos)',reportes:'Reportes (compartidos)'};
  const groups=new Map<string,PermissionGroup>();
  for(const permission of this.permissionCatalog()){
   let key=permission.codigo.startsWith('modulos.')?permission.codigo.split('.')[1]:permission.codigo.split('.')[0];
   if(key==='catalogos')key='administracion';
   if(!groups.has(key))groups.set(key,{key,label:labels[key]??key.replaceAll('_',' '),permissions:[]});
   groups.get(key)!.permissions.push(permission);
  }
  const term=this.permissionSearch.trim().toLocaleLowerCase('es');
  return [...groups.values()].sort((a,b)=>a.label.localeCompare(b.label,'es')).filter(g=>!term||g.label.toLocaleLowerCase('es').includes(term)||g.permissions.some(p=>`${p.codigo} ${this.permissionLabel(p)}`.toLocaleLowerCase('es').includes(term)));
 }
 permissionLabel(p:PermissionRow){
  if(p.codigo.startsWith('modulos.'))return 'Ver módulo';
  if(p.nombre&&p.nombre!==p.codigo)return p.nombre;
  const action=p.codigo.split('.').slice(1).join('.');
  const labels:Record<string,string>={consultar:'Consultar',administrar:'Administrar',crear:'Crear',solicitar:'Solicitar',aprobar:'Aprobar',aprobar_propia:'Aprobar solicitudes propias',montar:'Montar',ejecutar:'Ejecutar',gestionar:'Gestionar',descartar:'Descartar',opcionar:'Seleccionar opción',exportar:'Exportar',importar:'Importar',ver_todos:'Ver todos los centros',consultar_propias:'Consultar propias',reportar_inconsistencia:'Reportar inconsistencia',autorizar_inconsistencia_llanta:'Autorizar inconsistencia de llanta',gestionar_evidencias:'Gestionar evidencias'};
  return labels[action]??action.replaceAll('_',' ');
 }
 selectedPermissions(group:PermissionGroup){return group.permissions.filter(p=>this.permissionCodes.includes(p.codigo)).length}
 togglePermission(code:string,checked:boolean){this.permissionCodes=checked?[...new Set([...this.permissionCodes,code])]:this.permissionCodes.filter(p=>p!==code)}
 selectPermissionGroup(group:PermissionGroup,checked:boolean){for(const p of group.permissions)this.togglePermission(p.codigo,checked)}

 openUserImport(){this.showForm=false;this.importCsv='';this.importPassword='';this.importError='';this.importResult=null;this.showImport=true}
 closeUserImport(){if(this.importBusy)return;this.showImport=false;this.importCsv='';this.importPassword='';this.importResult=null;this.importError=''}
 async selectImportFile(event:Event){
  const file=(event.target as HTMLInputElement).files?.[0];this.importResult=null;this.importCsv='';this.importError='';this.importPassword='';
  if(!file)return;
  if(!file.name.toLowerCase().endsWith('.csv')||file.size>1000000){this.importError='Selecciona un CSV UTF-8 de máximo 1 MB y 1000 usuarios.';return}
  this.importBusy=true;try{
   this.importCsv=new TextDecoder('utf-8',{fatal:true}).decode(await file.arrayBuffer());
   this.importResult=await firstValueFrom(this.http.post<UserImportResult>('/api/usuarios/importar/validar',{csv:this.importCsv}));
  }catch(error:any){this.importError=error?.error?.message??error?.userMessage??'No se pudo validar. Revisa que el CSV esté codificado en UTF-8.'}finally{this.importBusy=false}
 }
 async importUsers(){if(this.importBusy||!this.importResult?.puedeImportar||!this.importPassword.trim())return;
  this.importBusy=true;this.importError='';try{
   const result=await firstValueFrom(this.http.post<{importados:number}>('/api/usuarios/importar',{csv:this.importCsv,password:this.importPassword}));
   this.importPassword='';this.importCsv='';this.importResult=null;this.showImport=false;
   this.message=`Se importaron ${result.importados} usuarios.`;setTimeout(()=>this.message='',5000);await this.load();
  }catch(error:any){
   if(Array.isArray(error?.error?.errores)){this.importResult=error.error;this.importError='No se importó ningún usuario. Corrige los errores y vuelve a seleccionar el archivo.'}
   else {this.importResult=null;this.importError=error?.error?.message??error?.userMessage??'No se pudo confirmar la importación. Vuelve a validar antes de reintentar.'}
  }finally{this.importPassword='';this.importBusy=false}
 }
 downloadImportErrors(){if(!this.importResult?.errores.length)return;
  const url=URL.createObjectURL(new Blob([JSON.stringify(this.importResult.errores,null,2)],{type:'application/json;charset=utf-8'}));
  const link=document.createElement('a');link.href=url;link.download='errores-usuarios.json';link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
 }
}
