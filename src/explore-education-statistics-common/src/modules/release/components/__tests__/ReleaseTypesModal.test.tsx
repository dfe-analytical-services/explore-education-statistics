import { render, screen, within } from '@testing-library/react';
import ReleaseTypesModal from '@common/modules/release/components/ReleaseTypesModal';
import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';

describe('ReleaseTypesModal', () => {
  test('renders the trigger button', () => {
    render(<ReleaseTypesModal />);

    expect(
      screen.getByRole('button', { name: 'What are release types?' }),
    ).toBeInTheDocument();
  });

  test('server-renders a no-JS fallback', () => {
    // Server HTML, before any effects run (so before the fallback is
    // swapped for the modal).
    const container = document.createElement('div');
    container.innerHTML = renderToStaticMarkup(<ReleaseTypesModal />);

    const fallback = within(container).getByTestId('modal-no-js-fallback');
    expect(fallback).toHaveTextContent('What are release types?');
    expect(fallback).toHaveTextContent('Accredited official statistics');
  });
});
