export type ComparisonOperator =
  | 'Equal'
  | 'NotEqual'
  | 'GreaterThan'
  | 'GreaterThanOrEqual'
  | 'LessThan'
  | 'LessThanOrEqual';

export type CardRarity = 'Common' | 'Uncommon' | 'Rare' | 'Special' | 'Mythic' | 'Bonus';

export type CardFormat =
  | 'Standard'
  | 'Future'
  | 'Historic'
  | 'Timeless'
  | 'Gladiator'
  | 'Pioneer'
  | 'Modern'
  | 'Legacy'
  | 'Pauper'
  | 'Vintage'
  | 'Penny'
  | 'Commander'
  | 'Oathbreaker'
  | 'StandardBrawl'
  | 'Brawl'
  | 'Alchemy'
  | 'PauperCommander'
  | 'Duel'
  | 'OldSchool'
  | 'Premodern'
  | 'Predh'
  | 'Custom';

export interface CardQuerySearch {
  name?: string;
  exactName?: boolean;
  color?: string;
  colorOperator?: ComparisonOperator;
  colorIdentity?: string;
  colorIdentityOperator?: ComparisonOperator;
  types?: string[];
  excludedTypes?: string[];
  oracleText?: string;
  keyword?: string;
  manaCost?: string;
  manaValue?: number;
  manaValueOperator?: ComparisonOperator;
  power?: string;
  powerOperator?: ComparisonOperator;
  toughness?: string;
  toughnessOperator?: ComparisonOperator;
  rarity?: CardRarity;
  rarityOperator?: ComparisonOperator;
  format?: CardFormat;
  pageSize?: number;
}
