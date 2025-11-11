import generateReleaseContent, {
  generateEditableRelease,
} from '@admin-test/generators/releaseContentGenerators';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import ReleasePageTabHome from '@admin/pages/release/content/components/ReleasePageTabHome';
import { ReleaseContentProvider } from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { ReleaseContent as ReleaseContentType } from '@admin/services/releaseContentService';
import render from '@common-test/render';
import { screen, within } from '@testing-library/react';
import React from 'react';
import { MemoryRouter } from 'react-router';

let mockIsMedia = false;
jest.mock('@common/hooks/useMedia', () => ({
  useMobileMedia: () => {
    return {
      isMedia: mockIsMedia,
    };
  },
}));

const testReleaseContent = generateReleaseContent({});
const renderWithContext = (
  component: React.ReactNode,
  releaseContent: ReleaseContentType = testReleaseContent,
) =>
  render(
    <ReleaseContentProvider
      value={{
        ...releaseContent,
        canUpdateRelease: true,
      }}
    >
      <EditingContextProvider editingMode="preview">
        <MemoryRouter>{component}</MemoryRouter>
      </EditingContextProvider>
      ,
    </ReleaseContentProvider>,
  );

describe('ReleasePageTabHome', () => {
  test('does not render summary block and publication summary on desktop', () => {
    renderWithContext(<ReleasePageTabHome hidden={false} />);

    expect(
      screen.queryByRole('heading', {
        level: 2,
        name: 'Introduction',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders summary block and publication summary on mobile', () => {
    mockIsMedia = true;
    renderWithContext(<ReleasePageTabHome hidden={false} />);

    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Introduction',
      }),
    ).toBeInTheDocument();

    mockIsMedia = false;
  });

  test('renders summary section if summary content exists', () => {
    renderWithContext(<ReleasePageTabHome hidden={false} />);

    expect(
      screen.getByRole('heading', {
        level: 2,
        name: 'Background information',
      }),
    ).toBeInTheDocument();
  });

  test('does not render summary section if no summary content', () => {
    renderWithContext(
      <ReleasePageTabHome hidden={false} />,
      generateReleaseContent({
        release: generateEditableRelease({
          summarySection: {
            id: 'summary-section-id',
            content: [],
            heading: 'Summary block heading',
            order: 0,
          },
        }),
      }),
    );

    expect(
      screen.queryByRole('heading', {
        level: 2,
        name: 'Background information',
      }),
    ).not.toBeInTheDocument();
  });

  test('renders headlines section', () => {
    renderWithContext(<ReleasePageTabHome hidden={false} />);
    const headlinesSection = screen.getByTestId('headlines-section');
    expect(headlinesSection).toBeInTheDocument();

    expect(
      within(headlinesSection).getByRole('heading', {
        level: 2,
        name: 'Headline facts and figures',
      }),
    ).toBeInTheDocument();
  });

  test('renders content sections as normal sections on desktop', () => {
    renderWithContext(<ReleasePageTabHome hidden={false} />);

    const content = screen.getByTestId('home-content');
    expect(content).toBeInTheDocument();

    expect(within(content).queryByTestId('accordion')).not.toBeInTheDocument();
    const sections = within(content).getAllByTestId('home-content-section');
    expect(sections).toHaveLength(2);
    expect(sections[0]).toHaveAttribute('id', 'section-content-section-1');
    expect(
      within(sections[0]).getByRole('heading', { level: 2 }),
    ).toHaveTextContent('Content section 1');
    expect(
      within(sections[0]).getByRole('heading', { level: 2 }),
    ).toHaveAttribute('id', 'heading-content-section-1');
  });

  test('renders content sections as accordions on mobile', () => {
    mockIsMedia = true;
    renderWithContext(<ReleasePageTabHome hidden={false} />);

    const content = screen.getByTestId('home-content');

    expect(within(content).getByTestId('accordion')).toBeInTheDocument();
    expect(
      within(content).queryByTestId('home-content-section'),
    ).not.toBeInTheDocument();
    mockIsMedia = false;
  });
});
