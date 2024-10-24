import Link from '@admin/components/Link';
import RelatedPageForm, {
  RelatedPageFormValues,
} from '@admin/pages/release/content/components/RelatedPageForm';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Modal from '@common/components/Modal';
import ReorderableList from '@common/components/ReorderableList';
import VisuallyHidden from '@common/components/VisuallyHidden';
import useToggle from '@common/hooks/useToggle';
import { BasicLink } from '@common/services/publicationService';
import reorder from '@common/utils/reorder';
import React, { useEffect, useState } from 'react';

interface Props {
  relatedPages: BasicLink[];
  onUpdate: (pages: BasicLink[]) => Promise<void>;
}

export default function RelatedPagesEditModal({
  relatedPages: initialRelatedPages,
  onUpdate,
}: Props) {
  const [open, toggleOpen] = useToggle(false);
  const [isReordering, toggleReordering] = useToggle(false);
  const [editingPage, setEditingPage] = useState<BasicLink>();
  const [relatedPages, setRelatedPages] =
    useState<BasicLink[]>(initialRelatedPages);

  useEffect(() => {
    setRelatedPages(initialRelatedPages);
  }, [initialRelatedPages]);

  const handleRemove = async (id: string) => {
    const updatedPages = relatedPages.filter(page => page.id !== id);
    await onUpdate(updatedPages);
  };

  const handleSubmit = async (values: RelatedPageFormValues) => {
    const updatedPages = relatedPages.map(page => {
      if (page.id === editingPage?.id) {
        return { id: editingPage.id, ...values };
      }
      return page;
    });
    await onUpdate(updatedPages);
    setEditingPage(undefined);
  };

  return (
    <Modal
      className="govuk-!-width-one-half"
      open={open}
      title="Edit related pages"
      triggerButton={
        <Button variant="secondary" onClick={toggleOpen.on}>
          Edit pages
        </Button>
      }
      onExit={toggleOpen.off}
    >
      {editingPage ? (
        <RelatedPageForm
          initialValues={{
            description: editingPage.description,
            url: editingPage.url,
          }}
          onCancel={() => setEditingPage(undefined)}
          onSubmit={handleSubmit}
        />
      ) : (
        <>
          {isReordering ? (
            <ReorderableList
              heading="Reorder pages"
              id="reorder-related-pages"
              list={relatedPages.map(page => ({
                id: page.id,
                label: page.description,
              }))}
              testId="reorder-related-pages"
              onCancel={() => {
                toggleReordering.off();
                setRelatedPages(initialRelatedPages);
              }}
              onConfirm={async () => {
                await onUpdate(relatedPages);
                toggleReordering.off();
              }}
              onMoveItem={({ prevIndex, nextIndex }) => {
                const reordered = reorder(relatedPages, prevIndex, nextIndex);
                setRelatedPages(reordered);
              }}
              onReverse={() => {
                setRelatedPages(relatedPages.toReversed());
              }}
            />
          ) : (
            <>
              <Button
                className="govuk-!-margin-bottom-3"
                variant="secondary"
                onClick={toggleReordering}
              >
                Reorder pages
              </Button>
              <table>
                <thead>
                  <tr>
                    <th>Title</th>
                    <th>Link URL</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {relatedPages.map(page => (
                    <tr key={page.id}>
                      <td>{page.description}</td>
                      <td>
                        <Link to={page.url}>{page.url}</Link>
                      </td>
                      <td>
                        <ButtonGroup className="govuk-!-margin-bottom-0">
                          <Button onClick={() => setEditingPage(page)}>
                            Edit
                            <VisuallyHidden> {page.description}</VisuallyHidden>
                          </Button>
                          <Button
                            variant="warning"
                            onClick={() => handleRemove(page.id)}
                          >
                            Remove{' '}
                            <VisuallyHidden> {page.description}</VisuallyHidden>
                          </Button>
                        </ButtonGroup>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <Button onClick={toggleOpen.off}>
                Close <VisuallyHidden> modal</VisuallyHidden>
              </Button>
            </>
          )}
        </>
      )}
    </Modal>
  );
}
