"""
Details of the Azure DevOps pipeline that a UI test run is part of.
"""

import os
from typing import Optional
from urllib.parse import quote


def get_pipeline_artifacts_url(collection_uri: str, project: str, build_id: str) -> Optional[str]:
    if not collection_uri or not project or not build_id:
        return None

    encoded_project = quote(project, safe="")
    encoded_build_id = quote(build_id, safe="")
    return (
        f"{collection_uri.rstrip('/')}/{encoded_project}/_build/results"
        f"?buildId={encoded_build_id}&view=artifacts&pathAsName=false&type=publishedArtifacts"
    )


def current_artifacts_url() -> Optional[str]:
    """
    Built from the variables that Azure DevOps predefines for every task. None outside
    of a pipeline, so that local test runs report without a link to artifacts that do
    not exist.
    """
    return get_pipeline_artifacts_url(
        collection_uri=os.getenv("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", ""),
        project=os.getenv("SYSTEM_TEAMPROJECT", ""),
        build_id=os.getenv("BUILD_BUILDID", ""),
    )
