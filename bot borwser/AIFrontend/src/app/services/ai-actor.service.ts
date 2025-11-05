import { Injectable } from '@angular/core';
import { gsap } from 'gsap';
import { ActionCommand } from './ai.service';

@Injectable({ providedIn: 'root' })
export class AiActorService {
  private cursorEl!: HTMLElement;

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
        case 'move':
          this.moveTo(action.selector!, action.durationMs ?? 600);
          await this.wait(action.durationMs ?? 600);
          break;

        case 'click':
          this.click(action.selector!);
          break;

        case 'type':
          await this.typeText(action.selector!, action.value ?? '');
          break;

        case 'select':
          this.selectValue(action.selector!, action.value ?? '');
          break;

        case 'scroll':
          window.scrollTo({ top: Number(action.value) || 0, behavior: 'smooth' });
          break;
      }
    }
  }

  private moveTo(selector: string, duration: number) {
    const el = document.querySelector(selector) as HTMLElement;
    if (!el) return;
    const rect = el.getBoundingClientRect();
    const x = rect.left + rect.width / 2;
    const y = rect.top + rect.height / 2;
    gsap.to(this.cursorEl, { x, y, duration: duration / 1000, ease: 'power2.out' });
  }

  private click(selector: string) {
    const el = document.querySelector(selector) as HTMLElement;
    el?.dispatchEvent(new MouseEvent('click', { bubbles: true }));
  }

  private async typeText(selector: string, text: string) {
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

  private selectValue(selector: string, value: string) {
    const el = document.querySelector(selector) as HTMLSelectElement | null;
    if (!el) return;
    el.value = value;
    el.dispatchEvent(new Event('change', { bubbles: true }));
  }

  private wait(ms: number) {
    return new Promise(res => setTimeout(res, ms));
  }
}
