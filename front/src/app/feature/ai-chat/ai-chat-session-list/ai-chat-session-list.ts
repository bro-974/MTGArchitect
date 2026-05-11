import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslocoPipe } from '@jsverse/transloco';
import { TreeNode } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TreeModule } from 'primeng/tree';
import { AiChatStateService } from '../ai-chat-state.service';
import { ChatSession } from '../ai-chat.models';

interface SessionNodeData {
  readonly sessionId: string;
}

@Component({
  selector: 'app-ai-chat-session-list',
  templateUrl: './ai-chat-session-list.html',
  styleUrl: './ai-chat-session-list.css',
  imports: [ButtonModule, InputTextModule, TreeModule, FormsModule, TranslocoPipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AiChatSessionList {
  public readonly chatState = inject(AiChatStateService);

  readonly editingId = signal<string | null>(null);
  readonly editingName = signal('');

  readonly treeNodes = computed<TreeNode<SessionNodeData>[]>(() =>
    this.chatState.sessions().map((s) => ({
      key: s.id,
      label: s.displayName,
      data: { sessionId: s.id },
      leaf: true,
    }))
  );

  readonly selectedNode = computed<TreeNode<SessionNodeData> | null>(() => {
    const active = this.chatState.activeSession();
    if (!active) return null;
    return this.treeNodes().find((n) => n.key === active.id) ?? null;
  });

  handleNodeSelect(event: { node: TreeNode<SessionNodeData> }): void {
    const sessionId = event.node.data?.sessionId;
    if (!sessionId || this.editingId() === sessionId) return;
    const session = this.chatState.sessions().find((s) => s.id === sessionId);
    if (session) {
      this.chatState.selectSession(session);
    }
  }

  handleNewChat(): void {
    this.chatState.createSession();
  }

  startRename(session: ChatSession, event: Event): void {
    event.stopPropagation();
    this.editingId.set(session.id);
    this.editingName.set(session.displayName);
  }

  commitRename(sessionId: string): void {
    const name = this.editingName().trim();
    if (name) {
      this.chatState.renameSession(sessionId, name);
    }
    this.editingId.set(null);
  }

  cancelRename(): void {
    this.editingId.set(null);
  }

  onDelete(sessionId: string, event: Event): void {
    event.stopPropagation();
    this.chatState.deleteSession(sessionId);
  }

  sessionFromNode(node: TreeNode<SessionNodeData>): ChatSession | undefined {
    return this.chatState.sessions().find((s) => s.id === node.data?.sessionId);
  }
}
