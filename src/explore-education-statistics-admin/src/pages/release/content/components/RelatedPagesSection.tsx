import Link from '@admin/components/Link';
import RelatedPagesEditModal from '@admin/pages/release/content/components/RelatedPagesEditModal';
import RelatedPagesAddModal from '@admin/pages/release/content/components/RelatedPagesAddModal';
import { RelatedPageFormValues } from '@admin/pages/release/content/components/RelatedPageForm';
import { useEditingContext } from '@admin/contexts/EditingContext';
import releaseContentRelatedInformationService from '@admin/services/releaseContentRelatedInformationService';
import { EditableRelease } from '@admin/services/releaseContentService';
import ButtonGroup from '@common/components/ButtonGroup';
import { BasicLink } from '@common/services/publicationService';
import React, { useState } from 'react';

interface Props {
  release: EditableRelease;
}

export default function RelatedPagesSection({ release }: Props) {
  const [relatedPages, setRelatedPages] = useState<BasicLink[]>(
    release.relatedInformation,
  );

  const { editingMode } = useEditingContext();

  const handleSubmit = async (values: RelatedPageFormValues) => {
    const updatedLinks = await releaseContentRelatedInformationService.update(
      release.id,
      [...relatedPages, values],
    );
    setRelatedPages(updatedLinks);
  };

  const handleUpdate = async (updatedPages: BasicLink[]) => {
    const updatedLinks = await releaseContentRelatedInformationService.update(
      release.id,
      updatedPages,
    );
    setRelatedPages(updatedLinks);
  };

  return (
    <>
      {(editingMode === 'edit' || relatedPages.length > 0) && (
        <h2 className="govuk-heading-s" id="related-pages">
          Related pages
        </h2>
      )}

      {relatedPages.length > 0 && (
        <ul className="govuk-list">
          {relatedPages.map(({ id, description, url }) => (
            <li key={id}>
              <Link to={url}>{description}</Link>
            </li>
          ))}
        </ul>
      )}

      {editingMode === 'edit' && (
        <ButtonGroup>
          <RelatedPagesAddModal onSubmit={handleSubmit} />
          {relatedPages.length > 0 && (
            <RelatedPagesEditModal
              relatedPages={relatedPages}
              onUpdate={handleUpdate}
            />
          )}
        </ButtonGroup>
      )}
    </>
  );
}
