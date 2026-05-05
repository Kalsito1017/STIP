import { describe, it, expect } from 'vitest';
import en from '../../i18n/locales/en.json';
import bg from '../../i18n/locales/bg.json';

function getAllKeys(obj: Record<string, unknown>, prefix = ''): string[] {
  const keys: string[] = [];
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      keys.push(...getAllKeys(value as Record<string, unknown>, fullKey));
    } else {
      keys.push(fullKey);
    }
  }
  return keys;
}

describe('i18n key parity', () => {
  it('bg.json has all the same keys as en.json', () => {
    const enKeys = getAllKeys(en).sort();
    const bgKeys = getAllKeys(bg).sort();

    const missingInBg = enKeys.filter(k => !bgKeys.includes(k));
    const extraInBg = bgKeys.filter(k => !enKeys.includes(k));

    expect(missingInBg).toEqual([]);
    expect(extraInBg).toEqual([]);
  });

  it('all en namespace keys have non-empty values', () => {
    const enKeys = getAllKeys(en);
    for (const key of enKeys) {
      const value = key.split('.').reduce((obj: unknown, part) => {
        if (obj && typeof obj === 'object') {
          return (obj as Record<string, unknown>)[part];
        }
        return undefined;
      }, en as unknown as Record<string, unknown>);
      expect(value, `Key "${key}" should have a non-empty value`).toBeTruthy();
    }
  });

  it('all bg namespace keys have non-empty values', () => {
    const bgKeys = getAllKeys(bg);
    for (const key of bgKeys) {
      const value = key.split('.').reduce((obj: unknown, part) => {
        if (obj && typeof obj === 'object') {
          return (obj as Record<string, unknown>)[part];
        }
        return undefined;
      }, bg as unknown as Record<string, unknown>);
      expect(value, `Key "${key}" should have a non-empty value`).toBeTruthy();
    }
  });
});
