import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';
import DOMPurify from 'dompurify';

@Pipe({
  name: 'markdown',
  standalone: true
})
export class MarkdownPipe implements PipeTransform {

  constructor(private sanitizer: DomSanitizer) {}

transform(value: string): SafeHtml {
    if (!value) return '';

    // Convert Markdown to HTML
    const html = marked.parse(value, { async: false }) as string;
    // Sanitize HTML (force return type as string)
    const safeHtml = DOMPurify.sanitize(html) as string;

    // Tell Angular this HTML is safe to render
    return this.sanitizer.bypassSecurityTrustHtml(safeHtml);
  }
}
