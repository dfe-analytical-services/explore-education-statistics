import TopicForm, {
  TopicFormValues,
} from '@admin/pages/themes/topics/components/TopicForm';
import render from '@common-test/render';
import { screen, waitFor } from '@testing-library/react';
import noop from 'lodash/noop';
import React from 'react';

describe('ThemeForm', () => {
  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<TopicForm onSubmit={handleSubmit} />);

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save topic' }));

    expect(
      await screen.findByText('Enter a title', {
        selector: '#topicForm-title-error',
      }),
    ).toBeInTheDocument();

    expect(handleSubmit).not.toHaveBeenCalled();
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    const { user } = render(<TopicForm onSubmit={handleSubmit} />);

    await user.type(screen.getByLabelText('Title'), 'Test title');

    expect(handleSubmit).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Save topic' }));

    await waitFor(() => {
      expect(handleSubmit).toHaveBeenCalledWith({
        title: 'Test title',
      } as TopicFormValues);
    });
  });

  describe('with `initialValues`', () => {
    test('renders correctly', async () => {
      render(
        <TopicForm
          initialValues={{
            title: 'Test title',
          }}
          onSubmit={noop}
        />,
      );

      await waitFor(() => {
        expect(screen.getByLabelText('Title')).toHaveValue('Test title');
      });
    });

    test('cannot submit with updated invalid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(<TopicForm onSubmit={handleSubmit} />);

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.clear(screen.getByLabelText('Title'));

      await user.click(screen.getByRole('button', { name: 'Save topic' }));

      expect(
        await screen.findByText('Enter a title', {
          selector: '#topicForm-title-error',
        }),
      ).toBeInTheDocument();

      expect(handleSubmit).not.toHaveBeenCalled();
    });

    test('can submit with updated valid values', async () => {
      const handleSubmit = jest.fn();

      const { user } = render(<TopicForm onSubmit={handleSubmit} />);

      await user.type(screen.getByLabelText('Title'), 'Updated title');

      expect(handleSubmit).not.toHaveBeenCalled();

      await user.click(screen.getByRole('button', { name: 'Save topic' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Updated title',
        } as TopicFormValues);
      });
    });
  });
});
