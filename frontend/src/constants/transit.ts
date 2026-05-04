export const TransitType = {
  Tram: 0,
  Metro: 1,
  Bus: 3,
  Trolley: 11,
} as const;

export const TransitTypeName: Record<number, string> = {
  [TransitType.Tram]: 'Tram',
  [TransitType.Metro]: 'Metro',
  [TransitType.Bus]: 'Bus',
  [TransitType.Trolley]: 'Trolley',
};

export const TransitTypeRouteColor: Record<number, string> = {
  [TransitType.Tram]: '#d97706',
  [TransitType.Metro]: '#2563eb',
  [TransitType.Bus]: '#16a34a',
  [TransitType.Trolley]: '#9333ea',
};

export const TransitTypeBadgeClass: Record<number, string> = {
  [TransitType.Tram]: 'bg-amber-100 text-amber-800',
  [TransitType.Metro]: 'bg-blue-100 text-blue-800',
  [TransitType.Bus]: 'bg-green-100 text-green-800',
  [TransitType.Trolley]: 'bg-purple-100 text-purple-800',
};
