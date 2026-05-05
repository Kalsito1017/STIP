import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"
import i18n from '../i18n'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function getLocale(): string {
  return i18n.language === 'bg' ? 'bg-BG' : 'en-US'
}
