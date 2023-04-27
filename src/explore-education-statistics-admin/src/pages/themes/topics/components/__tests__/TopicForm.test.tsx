import TopicForm, {
  TopicFormValues,
} from '@admin/pages/themes/topics/components/TopicForm';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import noop from 'lodash/noop';
import React from 'react';

describe('ThemeForm', () => {
  test('shows validation error when there is no title', async () => {
    render(<TopicForm onSubmit={noop} />);

    userEvent.click(screen.getByLabelText('Title'));
    userEvent.tab();

    await waitFor(() => {
      expect(
        screen.getByText('Enter a title', {
          selector: '#topicForm-title-error',
        }),
      ).toBeInTheDocument();
    });
  });

  test('cannot submit with invalid values', async () => {
    const handleSubmit = jest.fn();

    render(<TopicForm onSubmit={handleSubmit} />);

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save topic' }));

    await waitFor(() => {
      expect(handleSubmit).not.toHaveBeenCalled();
    });
  });

  test('can submit with valid values', async () => {
    const handleSubmit = jest.fn();

    render(<TopicForm onSubmit={handleSubmit} />);

    userEvent.type(screen.getByLabelText('Title'), 'Test title');

    expect(handleSubmit).not.toHaveBeenCalled();

    userEvent.click(screen.getByRole('button', { name: 'Save topic' }));

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

      render(<TopicForm onSubmit={handleSubmit} />);

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.clear(screen.getByLabelText('Title'));

      userEvent.click(screen.getByRole('button', { name: 'Save topic' }));

      await waitFor(() => {
        expect(handleSubmit).not.toHaveBeenCalled();
      });
    });

    test('can submit with updated valid values', async () => {
      const handleSubmit = jest.fn();

      render(<TopicForm onSubmit={handleSubmit} />);

      userEvent.type(screen.getByLabelText('Title'), 'Updated title');

      expect(handleSubmit).not.toHaveBeenCalled();

      userEvent.click(screen.getByRole('button', { name: 'Save topic' }));

      await waitFor(() => {
        expect(handleSubmit).toHaveBeenCalledWith({
          title: 'Updated title',
        } as TopicFormValues);
      });
    });
  });
});
