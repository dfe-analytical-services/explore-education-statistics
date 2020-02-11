import Button from '@common/components/Button';
import React, { useState } from 'react';
import { BasicMethodology } from 'src/services/common/types';

// interface FormValues {}

interface Props {
  methodology?: BasicMethodology;
}

const AssignMethodologyForm = ({ methodology }: Props) => {
  const [formOpen, setFormOpen] = useState(false);

  if (!formOpen)
    return (
      <>
        {methodology ? (
          <div>Current methodology: {methodology.title}</div>
        ) : (
          <div>This publication doesn't have a methodology.</div>
        )}
        <Button
          className="govuk-!-margin-top-6"
          onClick={() => setFormOpen(true)}
        >
          {!methodology ? 'Add' : 'Change'} methodology
        </Button>
      </>
    );
  return (
    <>
      <div className="govuk-!-margin-top-6">
        <Button
          className="govuk-!-margin-right-2"
          type="submit"
          onClick={() => setFormOpen(false)}
        >
          Submit
        </Button>
        <Button variant="secondary" onClick={() => setFormOpen(false)}>
          Cancel
        </Button>
      </div>
    </>
  );
};

export default AssignMethodologyForm;
