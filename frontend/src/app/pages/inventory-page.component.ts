import { Component, OnInit, inject } from '@angular/core';
import { RecordsApiService, RecordItem } from '../services/records-api.service';

@Component({
  standalone: true,
  selector: 'app-inventory-page',
  template: `
    <h1>Inventario</h1>
    <div class="card">
      <table style="width:100%">
        <thead>
          <tr><th>ID</th><th>Marca</th><th>Dimensión</th><th>Estado</th></tr>
        </thead>
        <tbody>
          @for (tire of tires; track tire.id) {
            <tr>
              <td>{{ tire.tireId }}</td>
              <td>{{ tire.brand }}</td>
              <td>{{ tire.dimension }}</td>
              <td>{{ tire.status }}</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `
})
export class InventoryPageComponent implements OnInit {
  private readonly api = inject(RecordsApiService);
  tires: RecordItem[] = [];

  ngOnInit(): void {
    this.api.getAll('tire').subscribe((records) => (this.tires = records));
  }
}
