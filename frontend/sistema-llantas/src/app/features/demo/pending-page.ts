import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({selector:'app-pending-page',template:`<section><h1>{{ title }}</h1><p>Módulo pendiente de implementación.</p><p>La trazabilidad por llanta está disponible en Llantas; los indicadores operativos están en Resumen. Esta pantalla no presenta datos simulados.</p></section>`})
export class PendingPage { readonly title = inject(ActivatedRoute).snapshot.data['title']; }
