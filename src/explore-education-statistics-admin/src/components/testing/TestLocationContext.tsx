import { useLocation } from 'react-router';
import React from 'react';
import { screen, waitFor } from '@testing-library/react';

/**
 * These are some helpers due to the changes in react router 6 and the inability to access
 * the location and history.
 */

export default function TestLocationContext() {
  const location = useLocation();
  return (
    <>
      <div data-testid="__current_pathname">{location.pathname}</div>
      <div data-testid="__current_hash">{location.hash}</div>
      <div data-testid="__current_search">{location.search}</div>
    </>
  );
}

function exactMatchFromString(str: string) {
  const escapedStr = str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  return new RegExp(`^${escapedStr}$`);
}

export async function expectLocation(url: string) {
  await waitFor(() => {
    expect(screen.getByTestId('__current_pathname')).toHaveTextContent(
      exactMatchFromString(url),
    );
  });
}

export async function expectLocationHash(hash: string) {
  await waitFor(() => {
    expect(screen.getByTestId('__current_hash')).toHaveTextContent(
      exactMatchFromString(hash),
    );
  });
}

export async function expectLocationSearch(search: string) {
  await waitFor(() => {
    expect(screen.getByTestId('__current_search')).toHaveTextContent(
      exactMatchFromString(search),
    );
  });
}
