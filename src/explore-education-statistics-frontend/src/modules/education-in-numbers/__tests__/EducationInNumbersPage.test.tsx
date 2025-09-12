import React from 'react';
import { render, screen, within } from '@testing-library/react';
import EducationInNumbersPage from '@frontend/modules/education-in-numbers/EducationInNumbersPage';
import {
  EinNavItem,
  EinPage,
} from '@frontend/services/educationInNumbersService';

describe('EducationInNumbersPage', () => {
  const testNavItemList: EinNavItem[] = [
    {
      id: 'root-id',
      order: 0,
      title: 'Education in numbers',
      slug: undefined,
      published: '2000-01-01',
    },
    {
      id: 'attendence-id',
      order: 1,
      title: 'Attendance',
      slug: 'attendance',
      published: '2001-02-02',
    },
  ];

  const testPageData: EinPage = {
    id: 'root-id',
    title: 'Education in Numbers',
    slug: undefined,
    description: 'Page description',
    published: '2023-08-29T10:00:00Z',
    content: [
      {
        id: 'section1-id',
        heading: 'Section 1',
        order: 0,
        content: [
          {
            id: 'block1-id',
            order: 0,
            type: 'HtmlBlock',
            body: '<p>Content 1</p>',
          },
        ],
      },
      {
        id: 'section2-id',
        heading: 'Section 2',
        order: 1,
        content: [
          {
            id: 'block2-id',
            order: 1,
            type: 'HtmlBlock',
            body: '<p>Content 2</p>',
          },
        ],
      },
    ],
  };

  test('renders the EiN root page correctly', () => {
    render(
      <EducationInNumbersPage
        pageData={testPageData}
        educationInNumbersPageList={testNavItemList}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Education in Numbers', level: 1 }),
    ).toBeInTheDocument();

    expect(
      screen.getByRole('navigation', { name: 'In this section' }),
    ).toBeInTheDocument();

    const contentSections = screen.getAllByTestId('ein-content-section');
    expect(contentSections).toHaveLength(2);
    expect(
      within(contentSections[0]).getByRole('heading', {
        name: 'Section 1',
        level: 2,
      }),
    );
    expect(
      within(contentSections[1]).getByRole('heading', {
        name: 'Section 2',
        level: 2,
      }),
    );
  });

  test('renders the EiN sub page correctly', () => {
    render(
      <EducationInNumbersPage
        pageData={{
          ...testPageData,
          id: 'attendance-id',
          title: 'Attendance page',
          slug: 'attendance',
        }}
        educationInNumbersPageList={testNavItemList}
      />,
    );

    expect(
      screen.getByRole('heading', { name: 'Attendance page', level: 1 }),
    ).toBeInTheDocument();
  });
});
