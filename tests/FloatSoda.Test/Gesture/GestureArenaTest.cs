using FloatSoda.Gesture;

namespace FloatSoda.Test.Gesture;

public class GestureArenaTest
{
    private sealed class FakeMember : IGestureArenaMember
    {
        public int Accepted { get; private set; }
        public int Rejected { get; private set; }
        public void AcceptGesture(int pointer) => Accepted++;
        public void RejectGesture(int pointer) => Rejected++;
    }

    [Fact]
    public void SoleMember_WinsOnClose()
    {
        var arena = new GestureArenaManager();
        var m = new FakeMember();

        arena.Add(1, m);
        arena.Close(1);

        Assert.Equal(1, m.Accepted);
        Assert.Equal(0, m.Rejected);
    }

    [Fact]
    public void FirstMember_WinsOnSweep_WhenUndecided()
    {
        var arena = new GestureArenaManager();
        var first = new FakeMember();
        var second = new FakeMember();

        arena.Add(1, first);
        arena.Add(1, second);
        arena.Close(1);   // 2人残りで未確定
        arena.Sweep(1);   // 先頭が既定勝者

        Assert.Equal(1, first.Accepted);
        Assert.Equal(1, second.Rejected);
    }

    [Fact]
    public void EagerAccept_AfterClose_WinsImmediately()
    {
        var arena = new GestureArenaManager();
        var winner = new FakeMember();
        var loser = new FakeMember();

        arena.Add(1, winner);
        arena.Add(1, loser);
        arena.Close(1);

        arena.Resolve(1, winner, GestureDisposition.Accepted);

        Assert.Equal(1, winner.Accepted);
        Assert.Equal(1, loser.Rejected);
        Assert.Equal(0, winner.Rejected);
    }

    [Fact]
    public void Reject_LeavesSoleSurvivor_WhoWins()
    {
        var arena = new GestureArenaManager();
        var survivor = new FakeMember();
        var quitter = new FakeMember();

        arena.Add(1, survivor);
        arena.Add(1, quitter);
        arena.Close(1);

        arena.Resolve(1, quitter, GestureDisposition.Rejected);

        Assert.Equal(1, quitter.Rejected);
        Assert.Equal(1, survivor.Accepted);
    }
}
