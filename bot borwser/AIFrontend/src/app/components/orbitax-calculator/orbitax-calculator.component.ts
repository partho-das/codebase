import { Component } from '@angular/core';
import {NgIf} from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-orbitax-calculator',
  templateUrl: './orbitax-calculator.component.html',
  imports: [
    NgIf, FormsModule
  ],
  standalone: true
})
export class OrbitaxCalculatorComponent {
  result = '';
revenue: any;
country: any;

  calculate() {
    const country = (document.getElementById('country-select') as HTMLSelectElement).value;
    const revenue = (document.getElementById('revenue-input') as HTMLInputElement).value;
    this.result = `Estimated minimum tax for ${country}: $${(Number(revenue) * 0.15).toFixed(2)}`;
  }
}
