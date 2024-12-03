// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.JavaScript.NodeApi.Runtime;

using static JSRuntime;

public sealed class NodejsEmbeddingRuntime
{
    private node_embedding_runtime _runtime;

    public NodejsEmbeddingRuntime(node_embedding_runtime runtime)
    {
        _runtime = runtime;
    }
}
