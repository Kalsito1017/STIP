import { describe, it, expect } from 'vitest';
import { TransitType, TransitTypeName, TransitTypeRouteColor, TransitTypeBadgeClass } from '../../constants/transit';

describe('transit constants', () => {
  it('TransitType has correct values', () => {
    expect(TransitType.Tram).toBe(0);
    expect(TransitType.Metro).toBe(1);
    expect(TransitType.Bus).toBe(3);
    expect(TransitType.Trolley).toBe(11);
  });

  it('TransitTypeName maps values to names', () => {
    expect(TransitTypeName[0]).toBe('Tram');
    expect(TransitTypeName[1]).toBe('Metro');
    expect(TransitTypeName[3]).toBe('Bus');
    expect(TransitTypeName[11]).toBe('Trolley');
  });

  it('TransitTypeRouteColor maps values to colors', () => {
    expect(TransitTypeRouteColor[0]).toBe('#d97706');
    expect(TransitTypeRouteColor[1]).toBe('#2563eb');
    expect(TransitTypeRouteColor[3]).toBe('#16a34a');
    expect(TransitTypeRouteColor[11]).toBe('#9333ea');
  });

  it('TransitTypeBadgeClass maps values to CSS classes', () => {
    expect(TransitTypeBadgeClass[0]).toContain('amber');
    expect(TransitTypeBadgeClass[1]).toContain('blue');
    expect(TransitTypeBadgeClass[3]).toContain('green');
    expect(TransitTypeBadgeClass[11]).toContain('purple');
  });
});
