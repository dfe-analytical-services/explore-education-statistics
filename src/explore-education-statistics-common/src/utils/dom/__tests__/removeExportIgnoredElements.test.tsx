import Details from '@common/components/Details';
import { render, screen } from '@testing-library/react';
import React from 'react';
import removeExportIgnoredElements, {
  exportIgnoreClassName,
} from '../removeExportIgnoredElements';

describe('removeExportIgnoredElements', () => {
  test('removes elements with the export ignore class', () => {
    const { container } = render(
      <div>
        <p>Keep 1</p>
        <p className={exportIgnoreClassName}>Remove 1</p>
        <div>
          <p>Keep 2</p>
          <p className={exportIgnoreClassName}>Remove 2</p>
        </div>
      </div>,
    );

    removeExportIgnoredElements(container);

    expect(screen.getByText('Keep 1')).toBeInTheDocument();
    expect(screen.getByText('Keep 2')).toBeInTheDocument();
    expect(screen.queryByText('Remove 1')).not.toBeInTheDocument();
    expect(screen.queryByText('Remove 2')).not.toBeInTheDocument();
  });

  test('removes the content of closed details elements', () => {
    const { container } = render(
      <details>
        <summary>Summary</summary>
        <p>Content</p>
      </details>,
    );

    removeExportIgnoredElements(container);

    expect(screen.getByText('Summary')).toBeInTheDocument();
    expect(screen.queryByText('Content')).not.toBeInTheDocument();
  });

  test('does not remove the content of open details elements', () => {
    const { container } = render(
      <details open>
        <summary>Summary</summary>
        <p>Content</p>
      </details>,
    );

    removeExportIgnoredElements(container);

    expect(screen.getByText('Summary')).toBeInTheDocument();
    expect(screen.getByText('Content')).toBeInTheDocument();
  });

  test('removes the content and close button from a closed Details component', () => {
    const { container } = render(
      <Details summary="Summary" showCloseButton>
        <p>Content</p>
      </Details>,
    );

    removeExportIgnoredElements(container);

    expect(screen.getByText('Summary')).toBeInTheDocument();
    expect(screen.queryByText('Content')).not.toBeInTheDocument();
    expect(screen.queryByText('Close')).not.toBeInTheDocument();
  });

  test('removes visually hidden elements', () => {
    const { container } = render(
      <Details summary="Summary" hiddenText="for Key stat" open>
        <p>Content</p>
      </Details>,
    );

    removeExportIgnoredElements(container);

    expect(screen.getByText('Summary')).toBeInTheDocument();
    expect(screen.getByText('Content')).toBeInTheDocument();
    expect(screen.queryByText('for Key stat')).not.toBeInTheDocument();
  });
});
