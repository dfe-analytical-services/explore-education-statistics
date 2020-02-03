import React from 'react';
import { Publication } from '@common/services/publicationService';
import Link from '@frontend/components/Link';

interface Props {
  publications: Publication[];
}

function MethodologyList({ publications }: Props) {
  const methodologySlugs: string[] = [];
  const methodologies = publications.map(({ methodology }) => {
    if (methodology && !methodologySlugs.includes(methodology.slug)) {
      methodologySlugs.push(methodology.slug);
      return methodology;
    }
    return null;
  });
  return (
    <ul className="govuk-list govuk-list--bullet">
      {methodologies.map(methodology => {
        return (
          methodology && (
            <div key={methodology.id}>
              <h3 className="govuk-heading-s govuk-!-margin-bottom-0">
                {methodology.title}
              </h3>
              <div className="govuk-!-margin-bottom-1">
                <Link to={`/methodology/${methodology.slug}`}>
                  View methodology
                </Link>
              </div>
            </div>
          )
        );
      })}
    </ul>
  );
}

export default MethodologyList;
