import {
  AfterViewInit,
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  ViewChild,
  inject,
  signal
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { MultiSelectFilter } from '../../shared/multi-select-filter';
import { DispositionApi } from './disposition-api';
import { EligibleTire, failure } from './disposition-models';

@Component({
  selector: 'app-disposition-proposal',
  imports: [FormsModule, MultiSelectFilter],
  templateUrl: './disposition-proposal.html',
  styleUrls: [
    './disposition-shared.scss',
    './disposition-dialog.scss'
  ]
})
export class DispositionProposal implements AfterViewInit, OnDestroy {
  @Input() mountCode = '';
  @Input() mountTire = '';
  @Input() mountPosition = '';

  @Output() closed = new EventEmitter<void>();
  @Output() created = new EventEmitter<void>();

  @ViewChild('dialog') dialog!: ElementRef<HTMLDialogElement>;

  private api = inject(DispositionApi);
  private version = 0;
  private timer?: ReturnType<typeof setTimeout>;

  rows = signal<EligibleTire[]>([]);
  selected = signal<EligibleTire[]>([]);
  busy = signal(false);
  loading = signal(false);
  error = signal('');

  page = 1;
  more = false;
  search = '';
  reason = '';
  notes = '';
  file: File | null = null;

  createdItems = signal<{ ordenId: string; llantaId: string }[]>([]);
  pendingUploads = signal<string[]>([]);

  keys = new Map<string, string>();
  cache = new Map<string, EligibleTire>();

  ngAfterViewInit() {
    this.dialog.nativeElement.showModal();
    this.search = this.mountCode;
    void this.load();
  }

  ngOnDestroy() {
    this.version++;
    clearTimeout(this.timer);
    this.dialog?.nativeElement?.close();
  }

  options() {
    return this.rows().map(t => ({
      value: t.id,
      label:
        `${t.codigo} · ${t.serial} · ${t.centro}` +
        (t.vehiculo ? ` · ${t.vehiculo} / ${t.posicion}` : '')
    }));
  }

  values() {
    return this.selected().map(t => t.id);
  }

  choose(ids: string[]) {
    if (this.mountTire) {
      ids = ids.filter(id => id === this.mountTire);
    }

    this.selected.set(
      ids
        .map(id => this.cache.get(id))
        .filter((t): t is EligibleTire => !!t)
    );

    for (const id of ids) {
      if (!this.keys.has(id)) {
        this.keys.set(id, crypto.randomUUID());
      }
    }
  }

  lookup(text: string) {
    this.search = text;
    clearTimeout(this.timer);
    this.timer = setTimeout(() => void this.load(), 250);
  }

  async load(more = false) {
    const v = ++this.version;
    const page = more ? this.page + 1 : 1;

    this.loading.set(true);
    this.error.set('');

    try {
      const p = await firstValueFrom(
        this.api.tires(this.search, page)
      );

      if (v !== this.version) return;

      for (const t of p.items) {
        this.cache.set(t.id, t);
      }

      if (
        this.mountTire &&
        p.items.some(t => t.id === this.mountTire) &&
        !this.selected().length
      ) {
        this.choose([this.mountTire]);
      }

      this.rows.set(
        more ? [...this.rows(), ...p.items] : p.items
      );
      this.page = page;
      this.more = p.pageNumber < p.totalPages;
    } catch (e) {
      if (v === this.version) {
        this.error.set(failure(e));
      }
    } finally {
      if (v === this.version) {
        this.loading.set(false);
      }
    }
  }

  async create() {
    if (
      this.busy() ||
      !this.selected().length ||
      this.selected().length > 100 ||
      !this.reason.trim() ||
      this.createdItems().length
    ) {
      return;
    }

    this.busy.set(true);
    this.error.set('');

    try {
      const items = this.selected().map(t => ({
        llantaId: t.id,
        ordenId: this.keys.get(t.id)!
      }));

      const created = await firstValueFrom(
        this.api.proposals(
          items,
          this.reason,
          this.notes,
          this.mountTire ? 'MONTAJE' : 'PROPUESTA',
          this.mountPosition || undefined
        )
      );

      this.createdItems.set(created);
      this.created.emit();

      if (this.file) {
        this.pendingUploads.set(
          created.map(i => i.ordenId)
        );
        await this.uploadPending();
      }
    } catch (e) {
      this.error.set(failure(e));
    } finally {
      this.busy.set(false);
    }
  }

  async uploadPending() {
    if (!this.file) return;

    this.busy.set(true);
    this.error.set('');

    const failed: string[] = [];

    try {
      for (const id of this.pendingUploads()) {
        try {
          await firstValueFrom(
            this.api.evidence(id, this.file)
          );
        } catch {
          failed.push(id);
        }
      }

      this.pendingUploads.set(failed);

      if (failed.length) {
        this.error.set(
          `Las propuestas están creadas. Falta cargar evidencia en ` +
          `${failed.length} órdenes; puedes reintentar sin duplicarlas.`
        );
      }
    } finally {
      this.busy.set(false);
    }
  }

  fileChanged(e: Event) {
    this.file =
      (e.target as HTMLInputElement).files?.[0] ?? null;
  }

  close() {
    if (!this.busy()) {
      this.closed.emit();
    }
  }
}