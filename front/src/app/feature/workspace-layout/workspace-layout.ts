import { Component } from '@angular/core';
import { SplitterModule } from 'primeng/splitter';
import { Workspace } from "../workspace/workspace";

@Component({
  selector: 'app-workspace-layout',
  templateUrl: './workspace-layout.html',
  styleUrl: './workspace-layout.css',
  imports: [SplitterModule, Workspace],
})
export class WorkspaceLayout {}
