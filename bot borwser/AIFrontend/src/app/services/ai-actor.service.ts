import { Injectable } from '@angular/core';
import { gsap } from 'gsap';
import { ActionCommand } from './ai.service';

export enum ActionType {
  Move = "Move",
  Click = "Click",
  Type = "Type",
  Select = "Select",
  Scroll = "Scroll",
  Wait = "Wait",
  ShowExplanation = "ShowExplanation"
}

@Injectable({ providedIn: 'root' })
export class AiActorService {
  private cursorEl!: HTMLElement;
  private defaultDuraton: number = 600;

  initCursor() {
    this.cursorEl = document.createElement('div');
    this.cursorEl.id = 'ai-cursor';
    this.cursorEl.className =
      'fixed top-0 left-0 w-4 h-4 bg-blue-500 rounded-full z-[9999] pointer-events-none';
    document.body.appendChild(this.cursorEl);
  }

  async runActions(actions: ActionCommand[]) {
    for (const action of actions) {
      switch (action.type) {
        case ActionType.Move:
          await this.moveTo(action.selector!, action.durationMs ?? 600);
          break;
        case ActionType.Click:
          await this.click(action.selector!);
          break;
        case ActionType.Type:
          await this.typeText(action.selector!, action.value ?? '');
          break;
        case ActionType.Select:
          await this.selectValue(action.selector!, action.value ?? '');
          break;
        case ActionType.Scroll:
          window.scrollTo({ top: Number(action.value) || 0, behavior: 'smooth' });
          break;
        case ActionType.Wait:
          await this.wait(action.durationMs ?? 600);
          break;
      }
    }
  }
  private moveTo(selector: string, duration: number) : Promise<void> {
    return new Promise( resolve => {
      const el = document.querySelector(selector) as HTMLElement;
      if (!el) return;

      const rect = el.getBoundingClientRect();
      const x = rect.left + rect.width / 2;
      const y = rect.top + rect.height / 2;

      // Get current cursor position
      const current = this.cursorEl.getBoundingClientRect();
      const dx = x - (current.left + current.width / 2);
      const dy = y - (current.top + current.height / 2);
      const distance = Math.sqrt(dx * dx + dy * dy);

      // Adjust duration dynamically for smoother effect
      const adjustedDuration = Math.min(Math.max(distance / 400, 0.6), 1.5);

      gsap.to(this.cursorEl, {
        x,
        y,
        duration: adjustedDuration,
        ease: 'power2.inOut',
        onComplete: ()=> resolve(),
      });
    });
  }


  private async click(selector: string) {
    await this.moveTo(selector, this.defaultDuraton);
    const el = document.querySelector(selector) as HTMLElement;
    el?.dispatchEvent(new MouseEvent('click', { bubbles: true }));
  }

  private async typeText(selector: string, text: string) {
    await this.moveTo(selector, this.defaultDuraton);
    const el = document.querySelector(selector) as HTMLInputElement | null;
    if (!el) return;
    el.focus();
    el.value = '';
    for (const char of text) {
      el.value += char;
      el.dispatchEvent(new Event('input', { bubbles: true }));
      await this.wait(100 + Math.random() * 100);
    }
  }

  private async selectValue(selector: string, value: string) {
    await this.moveTo(selector, this.defaultDuraton);
    const el = document.querySelector(selector) as HTMLSelectElement | null;
    if (!el) return;
    el.value = value;
    el.dispatchEvent(new Event('change', { bubbles: true }));
  }

  private wait(ms: number) {
    return new Promise(res => setTimeout(res, ms));
  }
}
