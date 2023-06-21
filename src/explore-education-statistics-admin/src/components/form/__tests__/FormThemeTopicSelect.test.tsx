import FormThemeTopicSelect from '@admin/components/form/FormThemeTopicSelect';
import { Theme } from '@admin/services/themeService';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import React from 'react';

describe('FormThemeTopicSelect', () => {
  const testThemes: Theme[] = [
    {
      id: 'theme-1',
      slug: 'theme-1',
      title: 'Theme 1',
      summary: '',
      topics: [
        {
          id: 'topic-1',
          slug: 'topic-1',
          title: 'Topic 1',
          themeId: 'theme-1',
        },
        {
          id: 'topic-2',
          slug: 'topic-2',
          title: 'Topic 2',
          themeId: 'theme-1',
        },
      ],
    },
    {
      id: 'theme-2',
      slug: 'theme-2',
      title: 'Theme 2',
      summary: '',
      topics: [
        {
          id: 'topic-3',
          slug: 'topic-3',
          title: 'Topic 3',
          themeId: 'theme-2',
        },
        {
          id: 'topic-4',
          slug: 'topic-4',
          title: 'Topic 4',
          themeId: 'theme-2',
        },
      ],
    },
  ];

  test('renders correctly with no theme', () => {
    render(
      <FormThemeTopicSelect id="themeTopic" legend="Theme/topic" themes={[]} />,
    );

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).queryAllByRole('option');

    expect(themes).toHaveLength(0);

    const topicSelect = screen.getByLabelText('Select topic');
    const topics = within(topicSelect).queryAllByRole('option');

    expect(topics).toHaveLength(0);
  });

  test('renders correctly with single theme and topic', () => {
    render(
      <FormThemeTopicSelect
        id="themeTopic"
        legend="Theme/topic"
        topicId="topic-1"
        themes={[
          {
            id: 'theme-1',
            slug: 'theme-1',
            title: 'Theme 1',
            summary: '',
            topics: [
              {
                id: 'topic-1',
                slug: 'topic-1',
                title: 'Topic 1',
                themeId: 'theme-1',
              },
            ],
          },
        ]}
      />,
    );

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(themes).toHaveLength(1);

    expect(themes[0]).toHaveTextContent('Theme 1');
    expect(themes[0]).toHaveValue('theme-1');
    expect(themes[0].selected).toBe(true);

    const topicSelect = screen.getByLabelText('Select topic');
    const topics = within(topicSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(topics).toHaveLength(1);

    expect(topics[0]).toHaveTextContent('Topic 1');
    expect(topics[0]).toHaveValue('topic-1');
    expect(topics[0].selected).toBe(true);
  });

  test('renders correctly with multiple themes and topics and no `topicId`', () => {
    render(
      <FormThemeTopicSelect
        id="themeTopic"
        legend="Theme/topic"
        themes={testThemes}
      />,
    );

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(themes).toHaveLength(2);

    expect(themes[0]).toHaveTextContent('Theme 1');
    expect(themes[0]).toHaveValue('theme-1');
    expect(themes[0].selected).toBe(true);

    expect(themes[1]).toHaveTextContent('Theme 2');
    expect(themes[1]).toHaveValue('theme-2');
    expect(themes[1].selected).toBe(false);

    const topicSelect = screen.getByLabelText('Select topic');
    const topics = within(topicSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(topics).toHaveLength(2);

    expect(topics[0]).toHaveTextContent('Topic 1');
    expect(topics[0]).toHaveValue('topic-1');
    expect(topics[0].selected).toBe(true);

    expect(topics[1]).toHaveTextContent('Topic 2');
    expect(topics[1]).toHaveValue('topic-2');
    expect(topics[1].selected).toBe(false);
  });

  test('renders correctly with multiple themes and topics and initial `topicId`', () => {
    render(
      <FormThemeTopicSelect
        id="themeTopic"
        legend="Theme/topic"
        topicId="topic-4"
        themes={testThemes}
      />,
    );

    const themeSelect = screen.getByLabelText('Select theme');
    const themes = within(themeSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(themes).toHaveLength(2);

    expect(themes[0]).toHaveTextContent('Theme 1');
    expect(themes[0]).toHaveValue('theme-1');
    expect(themes[0].selected).toBe(false);

    expect(themes[1]).toHaveTextContent('Theme 2');
    expect(themes[1]).toHaveValue('theme-2');
    expect(themes[1].selected).toBe(true);

    const topicSelect = screen.getByLabelText('Select topic');
    const topics = within(topicSelect).getAllByRole(
      'option',
    ) as HTMLOptionElement[];

    expect(topics).toHaveLength(2);

    expect(topics[0]).toHaveTextContent('Topic 3');
    expect(topics[0]).toHaveValue('topic-3');
    expect(topics[0].selected).toBe(false);

    expect(topics[1]).toHaveTextContent('Topic 4');
    expect(topics[1]).toHaveValue('topic-4');
    expect(topics[1].selected).toBe(true);
  });

  test('changing theme calls `onChange` handler with correct arguments', () => {
    const handleChange = jest.fn();

    render(
      <FormThemeTopicSelect
        id="themeTopic"
        legend="Theme/topic"
        themes={testThemes}
        onChange={handleChange}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    userEvent.selectOptions(screen.getByLabelText('Select theme'), ['theme-2']);

    expect(handleChange).toHaveBeenCalledTimes(1);
    expect(handleChange).toHaveBeenCalledWith('topic-3', 'theme-2');
  });

  test('changing topic calls `onChange` handler with correct arguments', () => {
    const handleChange = jest.fn();

    render(
      <FormThemeTopicSelect
        id="themeTopic"
        legend="Theme/topic"
        themes={testThemes}
        onChange={handleChange}
      />,
    );

    expect(handleChange).not.toHaveBeenCalled();

    userEvent.selectOptions(screen.getByLabelText('Select topic'), ['topic-2']);

    expect(handleChange).toHaveBeenCalledTimes(1);
    expect(handleChange).toHaveBeenCalledWith('topic-2', 'theme-1');
  });
});
