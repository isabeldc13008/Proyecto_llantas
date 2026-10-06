import {Routes} from '@angular/router';
export const DISPOSITION_ROUTES:Routes=[
 {path:'',pathMatch:'full',loadComponent:()=>import('./disposition-home').then(m=>m.DispositionHome)},
 {path:'bandeja',data:{kind:'ordenes'},loadComponent:()=>import('./disposition-inbox').then(m=>m.DispositionInbox)},
 {path:'ordenes/:id',loadComponent:()=>import('./disposition-order-detail').then(m=>m.DispositionOrderDetail)},
 {path:'lotes/nuevo',data:{kind:'lotes'},loadComponent:()=>import('./disposition-lot-editor').then(m=>m.DispositionLotEditor)},
 {path:'lotes/:id/recepcion',data:{receive:true},loadComponent:()=>import('./disposition-lot-detail').then(m=>m.DispositionLotDetail)},
 {path:'lotes/:id',loadComponent:()=>import('./disposition-lot-detail').then(m=>m.DispositionLotDetail)},
 {path:'lotes',data:{kind:'lotes'},loadComponent:()=>import('./disposition-inbox').then(m=>m.DispositionInbox)},
 {path:'despachos/nuevo',data:{kind:'despachos'},loadComponent:()=>import('./disposition-lot-editor').then(m=>m.DispositionLotEditor)},
 {path:'despachos/:id',loadComponent:()=>import('./disposition-dispatch-detail').then(m=>m.DispositionDispatchDetail)},
 {path:'despachos',data:{kind:'despachos'},loadComponent:()=>import('./disposition-inbox').then(m=>m.DispositionInbox)}
];
