import React from 'react';
import Link, { LinkProps } from '@admin/components/Link';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';

interface Props extends LinkProps {
  removeOnClick: () => void;
}

const EditableLink = ({ removeOnClick, ...props }: Props) => {
  return (
    <div>
      <div>
        <Link {...props} />
      </div>
      <Button
        className="govuk-!-margin-bottom-1"
        variant="secondary"
        onClick={() => removeOnClick()}
      >
        Remove link
      </Button>
    </div>
  );
};

export default wrapEditableComponent(EditableLink, Link);
