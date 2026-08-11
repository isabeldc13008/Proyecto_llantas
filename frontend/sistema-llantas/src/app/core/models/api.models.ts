export interface Page<T> { items:T[]; pageNumber:number; pageSize:number; totalItems:number; totalPages:number; }
export interface CatalogItem { id:string; codigo:string; nombre:string; activo:boolean; }
export interface Tire { id:string; codigo:string; serial:string; marca:string; referencia:string; dimension:string; tipo:string; estado:string; centro:string; ubicacionActual:string; profundidadInicial:number; activo:boolean; rowVersion:string; }
export interface TireInput { codigo:string; serial:string; marcaId:string; referenciaId:string; dimensionId:string; tipoLlantaId:string; estadoLlantaId:string; centroId:string; ubicacionActual:string; fechaCompra:string|null; costo:number|null; profundidadInicial:number; fechaIngreso:string|null; observaciones:string|null; rowVersion?:string; }
