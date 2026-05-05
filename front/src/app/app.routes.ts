import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { adminGuard } from './core/auth/admin.guard';

export const routes: Routes = [
	{
		path: '',
		loadComponent: () => import('./feature/home/home').then((module) => module.Home)
	},
	{
		path: 'feature/workspace',
		canActivate: [authGuard],
		loadComponent: () => import('./feature/workspace-layout/workspace-layout').then((module) => module.WorkspaceLayout)
	},
	{
		path: 'feature/login',
		loadComponent: () => import('./feature/login/login').then((module) => module.Login)
	},
	{
		path: 'feature/card-explorer',
		loadComponent: () =>
			import('./feature/card-explorer/card-explorer').then((module) => module.CardExplorer)
	},
	{
		path: 'feature/card-explorer-search-advanced',
		loadComponent: () =>
			import('./feature/card-explorer-search-advanced/card-explorer-search-advanced').then(
				(module) => module.CardExplorerSearchAdvanced
			)
	},
	{
		path: 'feature/server-status',
		canActivate: [adminGuard],
		loadComponent: () =>
			import('./feature/server-status/server-status').then((module) => module.ServerStatus)
	}
];
