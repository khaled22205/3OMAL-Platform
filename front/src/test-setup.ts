import 'zone.js';
import 'zone.js/testing';
import { NgModule } from '@angular/core';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserTestingModule,
  platformBrowserTesting,
} from '@angular/platform-browser/testing';
import { HttpClientModule } from '@angular/common/http';
import { HttpClientTestingModule } from '@angular/common/http/testing';

Object.defineProperty(globalThis, 'localStorage', {
  value: (() => {
    let store: Record<string, string> = {};
    return {
      getItem: (key: string) => store[key] ?? null,
      setItem: (key: string, value: string) => { store[key] = value; },
      removeItem: (key: string) => { delete store[key]; },
      clear: () => { store = {}; },
      get length() { return Object.keys(store).length; },
      key: (_: number) => null,
    };
  })(),
  writable: true,
  configurable: true,
});

@NgModule({
  imports: [
    BrowserTestingModule,
    HttpClientModule,
    HttpClientTestingModule,
  ],
})
class TestModule {}

getTestBed().initTestEnvironment(
  TestModule,
  platformBrowserTesting(),
);
