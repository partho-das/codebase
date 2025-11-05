import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import {OrbitaxCalculatorComponent} from './components/orbitax-calculator/orbitax-calculator.component';
import {ChatPanelComponent} from './components/chat-panel/chat-panel.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, OrbitaxCalculatorComponent, ChatPanelComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'AIFrontend';
}
