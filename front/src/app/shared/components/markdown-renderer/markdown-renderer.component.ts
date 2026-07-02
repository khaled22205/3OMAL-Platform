import { Component, input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-markdown-renderer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="markdown-content" [innerHTML]="sanitized()"></div>
  `,
  styles: [`
    .markdown-content :deep(p) { margin-bottom: 0.5rem; line-height: 1.6; }
    .markdown-content :deep(ul) { list-style: disc; padding-left: 1.5rem; margin-bottom: 0.5rem; }
    .markdown-content :deep(ol) { list-style: decimal; padding-left: 1.5rem; margin-bottom: 0.5rem; }
    .markdown-content :deep(li) { margin-bottom: 0.25rem; }
    .markdown-content :deep(strong) { font-weight: 700; }
    .markdown-content :deep(code) { background: rgba(0,0,0,0.06); padding: 0.1rem 0.3rem; border-radius: 0.25rem; font-size: 0.875em; }
    .markdown-content :deep(pre) { background: rgba(0,0,0,0.06); padding: 0.75rem; border-radius: 0.5rem; overflow-x: auto; margin-bottom: 0.5rem; }
    .markdown-content :deep(pre code) { background: none; padding: 0; }
    .markdown-content :deep(h3) { font-size: 1.05rem; font-weight: 700; margin-bottom: 0.5rem; margin-top: 0.75rem; }
    .markdown-content :deep(a) { color: #f97316; text-decoration: underline; }
    :host-context(.dark) .markdown-content :deep(code) { background: rgba(255,255,255,0.08); }
    :host-context(.dark) .markdown-content :deep(pre) { background: rgba(255,255,255,0.06); }
  `]
})
export class MarkdownRendererComponent {
  readonly content = input('');

  protected sanitized = computed(() => {
    return this.renderMarkdown(this.content());
  });

  private renderMarkdown(text: string): string {
    if (!text) return '';

    let html = this.escapeHtml(text);

    html = html.replace(/### (.+)/g, '<h3>$1</h3>');
    html = html.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
    html = html.replace(/\*(.+?)\*/g, '<em>$1</em>');
    html = html.replace(/`([^`]+)`/g, '<code>$1</code>');
    html = html.replace(/^- (.+)/gm, '<li>$1</li>');
    html = html.replace(/(<li>.*<\/li>\n?)+/g, '<ul>$&</ul>');
    html = html.replace(/^\d+\. (.+)/gm, '<li>$1</li>');
    html = html.replace(/(?:^|\n)(\[.*?\]\(.*?\))/g, (match) => {
      const linkMatch = match.match(/\[(.+?)\]\((.+?)\)/);
      if (linkMatch) return `<a href="${linkMatch[2]}" target="_blank" rel="noopener">${linkMatch[1]}</a>`;
      return match;
    });
    html = html.replace(/\n{2,}/g, '</p><p>');
    html = html.replace(/\n/g, '<br>');
    html = '<p>' + html + '</p>';
    html = html.replace(/<p><\/p>/g, '');
    html = html.replace(/<br><\/li>/g, '</li>');

    return html;
  }

  private escapeHtml(text: string): string {
    return text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }
}
