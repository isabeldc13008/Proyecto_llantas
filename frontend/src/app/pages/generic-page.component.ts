import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  standalone: true,
  selector: 'app-generic-page',
  template: `
    <h1>{{ title }}</h1>
    <div class="card">
      <p>Este módulo quedó migrado a Angular y está listo para implementar su lógica con ASP.NET Core.</p>
    </div>
  `
})
export class GenericPageComponent {
  private readonly route = inject(ActivatedRoute);
  readonly title = this.route.snapshot.data['title'] as string;
}
