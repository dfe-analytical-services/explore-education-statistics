import React from 'react';
import { usePrototypeNextSubjectContext } from '../contexts/PrototypeNextSubjectContext';

const PrototypeChangelogExample = () => {
  const {
    versionType,
    locations,
    filters,
    indicators,
  } = usePrototypeNextSubjectContext();
  return (
    <>
      <h3>Changelog</h3>

      {versionType === 'minor' && (
        <>
          <h4>Version notes</h4>
          <p>
            This is a minor update on the previous version, some new locations,
            filters and indicators have been added to the data set since the
            previous release, please see the details in the changelog below.
          </p>
        </>
      )}

      {locations.newItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">New locations</h4>

          <ul className="govuk-!-margin-bottom-6">
            {locations.newItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {locations.mappedItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Mapped locations</h4>

          <ul className="govuk-!-margin-bottom-6">
            {locations.mappedItems.map(item => (
              <li key={item[0].id}>
                {item[0].label} <strong>maps to</strong> {item[1].label}
              </li>
            ))}
          </ul>
        </>
      )}

      {locations.noMappingItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Unmapped locations</h4>

          <ul className="govuk-!-margin-bottom-6">
            {locations.noMappingItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {filters.newItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">New filters</h4>

          <ul className="govuk-!-margin-bottom-6">
            {filters.newItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {filters.mappedItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Mapped filters</h4>

          <ul className="govuk-!-margin-bottom-6">
            {filters.mappedItems.map(item => (
              <li key={item[0].id}>
                {item[0].label} <strong>maps to</strong> {item[1].label}
              </li>
            ))}
          </ul>
        </>
      )}

      {filters.noMappingItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Unmapped filters</h4>

          <ul className="govuk-!-margin-bottom-6">
            {filters.noMappingItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {indicators.newItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">New indicators</h4>

          <ul className="govuk-!-margin-bottom-6">
            {indicators.newItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}

      {indicators.mappedItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Mapped indicators</h4>

          <ul className="govuk-!-margin-bottom-6">
            {indicators.mappedItems.map(item => (
              <li key={item[0].id}>
                {item[0].label} <strong>maps to</strong> {item[1].label}
              </li>
            ))}
          </ul>
        </>
      )}

      {indicators.noMappingItems.length > 0 && (
        <>
          <h4 className="govuk-!-margin-bottom-0">Unmapped indicators</h4>

          <ul className="govuk-!-margin-bottom-6">
            {indicators.noMappingItems.map(item => (
              <li key={item.id}>{item.label}</li>
            ))}
          </ul>
        </>
      )}
    </>
  );
};

export default PrototypeChangelogExample;
