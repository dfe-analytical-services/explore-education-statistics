import React from 'react';
import { render, screen } from '@testing-library/react';
import { EinContentBlock } from '@common/services/types/einBlocks';
import EducationInNumbersContentSection from '@frontend/modules/education-in-numbers/components/EducationInNumbersContentSection';

describe('EducationInNumbersContentSection', () => {
  const testBlockContent: EinContentBlock[] = [
    {
      id: 'block1-id',
      order: 0,
      type: 'HtmlBlock',
      body: '<p>Content 1</p>',
    },
    {
      id: 'block2-id',
      order: 1,
      type: 'HtmlBlock',
      body: '<p>Content 2</p>',
    },
  ];

  test('renders the EiN content section correctly', () => {
    render(
      <EducationInNumbersContentSection
        content={testBlockContent}
        heading="Section 1"
        isLastSection={false}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Section 1', level: 2 }),
    ).toBeInTheDocument();

    const section = screen.getByTestId('ein-content-section');
    expect(section).toHaveTextContent('Content 1');
    expect(section).toHaveTextContent('Content 2');
  });
});
