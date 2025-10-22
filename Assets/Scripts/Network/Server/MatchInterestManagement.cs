using System.Collections.Generic;
using System.Linq;
using Mirror;

// InterestManagementBase���p�����邱�ƂŁANetworkManager��AOI(�����͈�)���[���Ƃ��ċ@�\����
public class MatchInterestManagement : InterestManagementBase
{
    // [Server]
    // ���̃I�u�W�F�N�g��N������ׂ����𔻒肷��u���[���u�b�N�v
    public override bool OnCheckObserver(NetworkIdentity identity, NetworkConnectionToClient newObserver)
    {
        // �ώ@��(�N���C�A���g)�̃v���C���[�I�u�W�F�N�g���Ȃ���΁A���������Ȃ�
        if (newObserver.identity == null) return false;

        // �`�F�b�N�Ώۂ̃I�u�W�F�N�g��PlayerConnection�̏ꍇ
        if (identity.TryGetComponent<PlayerState>(out var targetPlayerState))
        {
            // PlayerConnection�́A���̏��L�Ҏ��g�ɂ��������Ȃ�
            return targetPlayerState.connectionToClient == newObserver;
        }

        // �`�F�b�N�Ώۂ̃I�u�W�F�N�g���L�����N�^�[�̏ꍇ
        if (identity.TryGetComponent<NetworkPlayerController>(out var targetCharacter))
        {
            // �ώ@��(�N���C�A���g)���L�����N�^�[�łȂ���΁A�����Ȃ�
            if (!newObserver.identity.TryGetComponent<NetworkPlayerController>(out var observerCharacter))
                return false;

            // �������̃L�����N�^�[�́A��������ID�����l�ɂ���������
            return targetCharacter.matchId == observerCharacter.matchId && targetCharacter.matchId != 0;
        }

        // ��L�ȊO�̃l�b�g���[�N�I�u�W�F�N�g�́A�f�t�H���g�Ō�����
        return true;
    }

    // [Server]
    // �I�u�W�F�N�g�̌��݂̊ώ@�҃��X�g���č\�z����u�A�N�V�����v
    public override void Rebuild(NetworkIdentity identity, bool initialize)
    {
        // �V�����ώ@�҃��X�g���쐬
        HashSet<NetworkConnectionToClient> newObservers = new HashSet<NetworkConnectionToClient>();

        // �ڑ����Ă���S�ẴN���C�A���g���`�F�b�N���A���[���u�b�N(OnCheckObserver)�ɏ]���ă��X�g�ɒǉ�
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn != null && conn.identity != null && OnCheckObserver(identity, conn))
            {
                newObservers.Add(conn);
            }
        }

        // �Â����X�g�ƐV�������X�g���r���A������K�p����

        // �V����������悤�ɂȂ����l��T��
        foreach (var newObserver in newObservers)
        {
            if (!identity.observers.ContainsKey(newObserver.connectionId))
            {
                // ���X�g�ɒǉ����A�X�|�[�����b�Z�[�W�𑗐M
                AddObserver(newObserver, identity);
            }
        }

        // �����Ȃ��Ȃ����l��T��
        List<NetworkConnection> oldObservers = new List<NetworkConnection>(identity.observers.Values);
        foreach (var oldObserver in oldObservers)
        {
            if (!newObservers.Contains(oldObserver))
            {
                // ���X�g����폜���A��\�����b�Z�[�W�𑗐M
                RemoveObserver(oldObserver as NetworkConnectionToClient, identity);
            }
        }
    }
}