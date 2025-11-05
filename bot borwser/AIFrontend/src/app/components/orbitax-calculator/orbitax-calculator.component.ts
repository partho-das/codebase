import { Component } from '@angular/core';
import {NgIf} from '@angular/common';

@Component({
  selector: 'app-orbitax-calculator',
  templateUrl: './orbitax-calculator.component.html',
  imports: [
    NgIf
  ],
  standalone: true
})
export class OrbitaxCalculatorComponent {
  result = '';

  calculate() {
    const country = (document.getElementById('country-select') as HTMLSelectElement).value;
    const revenue = (document.getElementById('revenue-input') as HTMLInputElement).value;
    this.result = `Estimated minimum tax for ${country}: $${(Number(revenue) * 0.15).toFixed(2)}`;
  }
}
