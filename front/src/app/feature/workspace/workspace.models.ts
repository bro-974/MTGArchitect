export interface WorkspaceQuerySearch {
  readonly id: string;
  readonly query: string;
  readonly searchEngine: string;
}

export interface WorkspaceDeckCard {
  readonly id: string;
  readonly cardName: string;
  readonly scryFallId: string;
  readonly quantity: number;
  readonly type: string;
  readonly cost: string | null;
  readonly isSideBoard: boolean;
}

export interface WorkspaceDeck {
  readonly id: string;
  readonly name: string;
  readonly type: string;
  readonly note: string | null;
  readonly querySearches: readonly WorkspaceQuerySearch[];
  readonly cards: readonly WorkspaceDeckCard[];
}

export interface WorkspaceQuerySearchUpsert {
  readonly id: string | null;
  readonly query: string;
  readonly searchEngine: string;
}

export interface WorkspaceDeckCardAdd {
  readonly cardName: string;
  readonly scryFallId: string;
  readonly quantity: number;
  readonly type: string;
  readonly cost: string | null;
  readonly isSideBoard: boolean;
}

export interface WorkspaceDeckCardUpsert {
  readonly cardName: string;
  readonly scryFallId: string;
  readonly quantity: number;
  readonly type: string;
  readonly cost: string | null;
  readonly isSideBoard: boolean;
}

export interface WorkspaceDeckUpsert {
  readonly name: string;
  readonly type: string;
  readonly format: string | null;
  readonly commander: string | null;
  readonly colorIdentity: string | null;
  readonly note: string | null;
  readonly querySearches: readonly WorkspaceQuerySearchUpsert[];
  readonly cards: readonly WorkspaceDeckCardUpsert[];
}