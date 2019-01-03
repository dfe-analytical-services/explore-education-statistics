import React, { Component } from 'react';
import PropTypes from 'prop-types'

class Title extends Component {
    render() {
        return (
            <div className="app-content__header">
                <span className="govuk-caption-xl">{this.props.label}</span>
                <h1 className="govuk-heading-xl">Find {(this.props.label || '').toLowerCase()} statistics</h1>
            </div>
        );
    }
}

Title.propTypes = {
    label: PropTypes.string
}

Title.defaultProps = {
    label: ''
}

export default Title;