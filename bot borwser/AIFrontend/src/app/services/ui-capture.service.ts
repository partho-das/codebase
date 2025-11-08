import { Injectable } from "@angular/core";
import { HttpClient, HttpContext } from "@angular/common/http";
import { AiResponse } from "./ai.service";

export interface UiElementSnapshot {
  logicalId?: string; 
  tag: string;
  text?: string;
  placeholder?: string;
  name?: string;
  idAttr?: string;
  classes?: string;
  role?: string;
  type?: string;
  bounding?: { top:number,left:number,width:number,height:number };
}

@Injectable({providedIn: 'root'})
export class UiCaptureService{
  constructor(private http: HttpClient){}
  

  capture(): UiElementSnapshot[] {
    const sel = 'button,input,select,textarea,[data-ai]';
    const els = Array.from(document.querySelectorAll(sel));
    return els.map((el): UiElementSnapshot => {
      const r = (el as HTMLElement).getBoundingClientRect();
      return {
        logicalId: (el as HTMLElement).dataset['ai'] || (el as HTMLElement).id || undefined,
        tag: el.tagName.toLowerCase(),
        text: (el.textContent || '').trim().slice(0,200),
        placeholder: (el as HTMLInputElement).placeholder || undefined,
        name: (el as HTMLInputElement).name || undefined,
        idAttr: (el as HTMLElement).id || undefined,
        classes: (el as HTMLElement).className || undefined,
        role: (el as HTMLElement).getAttribute('role') || undefined,
        type: (el as HTMLInputElement).type || undefined,
        bounding: { top: Math.round(r.top), left: Math.round(r.left), width: Math.round(r.width), height: Math.round(r.height) }
      };
    });
  }

  requestAiActon(message: string){
    const snapshot = this.capture();
    const snapshotString = JSON.stringify(snapshot);
    return this.http.post<AiResponse>("/api/ai/agent/", {message, snapshotString});
  }

}