import React, { useContext } from 'react';
import Link, { LinkProps } from '@admin/components/Link';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import Button from '@common/components/Button';

interface Props extends LinkProps {
  to: string;
  removeOnClick: () => void;
}

const EditableLink = ({
  removeOnClick,
  to: href,
  children,
  ...props
}: Props) => {
  const { isEditing } = useContext(EditingContext);

  return !isEditing ? (
    <Link
      to=""
      href={href}
      {...props}
      rel="noopener noreferrer"
      target="_blank"
    >
      {children}
    </Link>
  ) : (
    <div>
      <div>
        <Link
          to=""
          href={href}
          {...props}
          rel="noopener noreferrer"
          target="_blank"
        >
          {children}
        </Link>
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

export default EditableLink;
