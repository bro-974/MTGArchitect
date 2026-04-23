import {
  ChangeDetectionStrategy,
  Component,
  inject
} from '@angular/core';
import { SplitterModule } from 'primeng/splitter';
import { DeckForm } from '../deck-form/deck-form';
import { WorkspaceDeckSelected } from '../workspace-deck-selected/workspace-deck-selected';
import { Workspace } from '../workspace/workspace';
import { WorkspaceLayoutStateService } from './workspace-layout-state.service';

@Component({
  selector: 'app-workspace-layout',
  templateUrl: './workspace-layout.html',
  styleUrl: './workspace-layout.css',
  imports: [
    SplitterModule,
    Workspace,
    WorkspaceDeckSelected,
    DeckForm
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WorkspaceLayout {
  private readonly layoutStateService = inject(WorkspaceLayoutStateService);

  readonly isCreateDeckVisible = this.layoutStateService.isCreateDeckVisible;

  handleCreateDeckRequest(): void {
    this.layoutStateService.openCreateDeckForm();
  }
}
