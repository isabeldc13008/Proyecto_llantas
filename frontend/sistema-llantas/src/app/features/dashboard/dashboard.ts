import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({ selector:'app-dashboard', imports:[RouterLink], template:`
<section class="hero page"><div><p class="eyebrow">GLLD · EDINSA</p><h1>Cada llanta.<br><em>Toda su historia.</em></h1><p>Gestión logística de llantas para la operación de transporte de Postobón.</p><a routerLink="/llantas">Abrir inventario →</a></div><div class="wheel" aria-hidden="true"><span></span></div></section>
<section class="page stats"><article><b>20K</b><span>Capacidad inicial</span></article><article><b>100%</b><span>Trazabilidad</span></article><article><b>24/7</b><span>Visibilidad operativa</span></article></section>`, styles:[`
.hero{min-height:66vh;display:grid;grid-template-columns:1.1fr .9fr;align-items:center;gap:3rem;background:radial-gradient(circle at 90% 10%,#d6e7df 0,transparent 40%)}
h1 em{color:#a67a00;font-style:normal}.hero p:not(.eyebrow){max-width:570px;font-size:1.15rem;color:#617571}.hero a{display:inline-block;margin-top:1rem;color:#183a36;font-weight:800;text-decoration:none;border-bottom:3px solid #e0b72c;padding-bottom:.2rem}
.wheel{width:min(35vw,420px);aspect-ratio:1;border-radius:50%;background:repeating-conic-gradient(#1b2d2b 0 5deg,#314542 5deg 9deg);padding:55px;box-shadow:20px 30px 70px #16352c33;transform:rotate(-12deg)}.wheel span{display:block;width:100%;height:100%;border:30px solid #9daaa6;border-radius:50%;background:radial-gradient(circle,#172a28 0 13%,#d8dedb 14% 38%,#253a37 39%)}
.stats{display:grid;grid-template-columns:repeat(3,1fr);gap:1rem;margin-top:-2rem}.stats article{background:#fff;padding:1.5rem;border-radius:18px;border:1px solid #dfe8e4;display:flex;flex-direction:column}.stats b{font-size:2rem;color:#193d39}.stats span{color:#71827f}@media(max-width:760px){.hero{grid-template-columns:1fr}.wheel{display:none}.stats{grid-template-columns:1fr;margin-top:0}}
`]})
export class Dashboard {}
